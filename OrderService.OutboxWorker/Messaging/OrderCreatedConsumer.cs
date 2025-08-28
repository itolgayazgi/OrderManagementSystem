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
using OrderService.Application.Events;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;

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

            // Declare & bind - Async versiyonları kullanın
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
                    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                    var invoiceGw = scope.ServiceProvider.GetRequiredService<IInvoiceGateway>();

                    // Inbox idempotency: aynı messageId işlendiyse atla
                    var already = await db.InboxMessage.AnyAsync(x => x.MessageId == messageId, stoppingToken);

                    if (already)
                    {
                        await _ch!.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                        return;
                    }

                    var evt = JsonSerializer.Deserialize<OrderCreatedEvent>(json)!;

                    await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);

                    // 1) Stok düş (Product tablosunda Quantity/Stock sütunu olduğunu varsayıyorum)
                    var productIds = evt.Items.Select(x => x.ProductId).ToList();
                    var products = await db.Product
                        .Where(p => productIds.Contains(p.Id))
                        .ToListAsync(stoppingToken);

                    foreach (var it in evt.Items)
                    {
                        var p = products.FirstOrDefault(x => x.Id == it.ProductId);
                        if (p.StockQuantity < it.Quantity)
                            throw new InvalidOperationException($"{p.Sku} kodunda yeterli stok yok");

                        p.DecreaseStock(it.Quantity); // domain metodun yoksa: p.StockQuantity -= it.Quantity;
                    }

                    // 2) Siparişi Completed yap
                    var order = await db.Order
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == evt.OrderId, stoppingToken);
                    order.MarkCompleted();

                    var (invoiceNo, extTrace) = await invoiceGw.CreateAsync(order.Id, stoppingToken);
                    // 3) Fatura oluştur (dış servis mock)
                    var inv = new Invoice(order.Id, invoiceNo, extTrace);
                    db.Invoice.Add(inv);

                    // dış servisi simüle et
                    var latency = int.TryParse(_cfg["Invoice:SimulatedLatencyMs"], out var l) ? l : 400;
                    await Task.Delay(latency, stoppingToken);
                    inv.MarkCompleted();

                    // 4) Inbox kaydı (idempotency)
                    db.InboxMessage.Add(new InboxMessage(messageId, "order.created"));

                    await db.SaveChangesAsync(stoppingToken);
                    await tx.CommitAsync(stoppingToken);

                    await _ch!.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Consumer] Error: {ex.Message}");
                    // retry/backoff kurguna göre: Nack + requeue true. (Şimdilik requeue=true)
                    await _ch!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
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
