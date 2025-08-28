using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace OrderService.Infrastructure.Messaging
{
    public sealed class RabbitPublisher : IRabbitPublisher, IDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _conn;
        private IChannel? _ch;

        public RabbitPublisher(IConfiguration cfg)
        {
            _factory = new ConnectionFactory
            {
                HostName = cfg["Rabbit:Host"] ?? "localhost",
                UserName = cfg["Rabbit:User"] ?? "admin",
                Password = cfg["Rabbit:Pass"] ?? "admin",
                Port = int.TryParse(cfg["Rabbit:Port"], out var p) ? p : 5672,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
        }

        private async Task EnsureOpenAsync(CancellationToken ct)
        {
            if (_conn is { IsOpen: true } && _ch is { IsOpen: true })
                return;

            _ch?.Dispose();
            _conn?.Dispose();

            // Düzeltme 1: await anahtar sözcüğü kullanılmalı
            _conn = await _factory.CreateConnectionAsync(ct);
            _ch = await _conn.CreateChannelAsync();

            // Düzeltme 2: ExchangeDeclareAsync kullanılmalı (7.x'te async)
            await _ch.ExchangeDeclareAsync(
                exchange: "order-ex",
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);
        }

        public async Task PublishAsync(string exchange, string routingKey, string message, CancellationToken ct, string? messageId = null)
        {
            await EnsureOpenAsync(ct);

            var props = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent, // Düzeltme 3: DeliveryMode enum kullanımı
                MessageId = messageId
            };

            // Düzeltme 4: BasicPublishAsync kullanılmalı (7.x'te async)
            ReadOnlyMemory<byte> body = Encoding.UTF8.GetBytes(message).AsMemory();

            await _ch!.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);
        }

        public void Dispose()
        {
            _ch?.Dispose();
            _conn?.Dispose();
        }
    }

    public interface IRabbitPublisher : IDisposable
    {
        Task PublishAsync(string exchange, string routingKey, string message, CancellationToken ct, string? messageId = null);
    }
}