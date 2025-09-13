using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using OrderService.Domain.Entity;
using OrderService.Infrastructure.Persistence;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Interfaces;
using MediatR;
using OrderService.Application.IntegrationEvents;

namespace OrderService.OutboxWorker.Messaging
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IConfiguration _cfg;

        private IConnection? _conn;
        private IChannel? _ch;

        public OrderCreatedConsumer(IServiceProvider sp, IConfiguration cfg)
        {
            _sp = sp;
            _cfg = cfg;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _cfg["Rabbit:Host"] ?? "localhost",
                UserName = _cfg["Rabbit:User"] ?? "admin",
                Password = _cfg["Rabbit:Pass"] ?? "admin",
                Port = int.TryParse(_cfg["Rabbit:Port"], out var p) ? p : 5672,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            _conn = await factory.CreateConnectionAsync(stoppingToken);
            _ch = await _conn.CreateChannelAsync(cancellationToken: stoppingToken);

            var exchange = _cfg["Rabbit:Exchange"] ?? "order-ex";
            var queue = _cfg["Rabbit:Queue"] ?? "q-order-created";
            var rk = _cfg["Rabbit:RoutingKey"] ?? "order.created";

            await _ch.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: stoppingToken);
            await _ch.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await _ch.QueueBindAsync(queue, exchange, rk, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_ch);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
                var json = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                try
                {
                    using var scope = _sp.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                    // Inbox idempotency kontrolü (bu kısım doğru)
                    var alreadyProcessed = await db.InboxMessage
                                                   .AnyAsync(x => x.MessageId == messageId, stoppingToken);

                    if (alreadyProcessed)
                    {
                        await _ch!.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    var notification = JsonSerializer.Deserialize<OrderCreatedNotification>(json)!;

                    // --- DOĞRU VE SAĞLAM YAPI ---

                    // 1. Transaction'ı başlat.
                    await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);

                    // 2. Inbox'a eklenecek mesajı HAZIRLA (henüz kaydetme).
                    db.InboxMessage.Add(new InboxMessage(messageId, "order.created"));

                    // 3. Business logic'i çalıştır, o da kendi değişikliklerini HAZIRLASIN.
                    //    (Stok güncellemesi ve yeni Invoice eklemesi DbContext'e eklenir).
                    await mediator.Publish(notification, stoppingToken);

                    // 4. ŞİMDİ, tüm biriken değişiklikleri (Inbox + Stok + Fatura) tek seferde kaydet.
                    await db.SaveChangesAsync(stoppingToken);

                    // 5. Her şey başarıyla kaydedildiyse, transaction'ı onayla.
                    await tx.CommitAsync(stoppingToken);

                    // 6. RabbitMQ'ya mesajın işlendiğini bildir.
                    await _ch!.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    // ... catch bloğunuz aynı kalabilir ...
                }
            };

            await _ch.BasicQosAsync(prefetchSize: 0, prefetchCount: 16, global: false, cancellationToken: stoppingToken);
            await _ch.BasicConsumeAsync(queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            Console.WriteLine("[Consumer] Listening order.created …");

            // servis yaşadığı sürece bekle
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        public override void Dispose()
        {
            try
            {
                _ch?.CloseAsync().GetAwaiter().GetResult();
            }
            catch { }

            try
            {
                _conn?.CloseAsync().GetAwaiter().GetResult();
            }
            catch { }

            _ch?.Dispose();
            _conn?.Dispose();
            base.Dispose();
        }
    }
}
