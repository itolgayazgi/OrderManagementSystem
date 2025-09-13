using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;

namespace OrderService.OutboxWorker.Messaging
{
    public class OutboxDispatcher  : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IRabbitPublisher _publisher;
        private readonly IConfiguration _cfg;


        public OutboxDispatcher(IServiceProvider sp, IRabbitPublisher publisher, IConfiguration cfg)
        {
            _sp = sp;
            _publisher = publisher;
            _cfg = cfg;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var ex = _cfg["Rabbit:Exchange"] ?? "order-ex";
            var rk = _cfg["Rabbit:RoutingKey"] ?? "order.created";

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(ct))
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                var now = DateTime.UtcNow;  
                // İşlenmemiş outbox mesajlarından küçük bir batch
                var batch = await db.OutboxMessage
                    .Where(x => x.ProcessedAt == null && (x.ProcessAt == null || x.ProcessAt <= now) )
                    .OrderBy(x => x.CreatedAt)
                    .Take(50)
                    .ToListAsync(ct);

                foreach (var msg in batch)
                {
                    try
                    {
                        // messageId olarak DB Id'sini kullanıyoruz (idempotent delivery için işe yarar)
                        //await _publisher.PublishAsync(ex, msg.EventType, msg.Payload, ct, messageId: msg.Id.ToString()); old version
                        await _publisher.PublishAsync(ex, rk, msg.Payload, ct, messageId: msg.Id.ToString()); // routing key test
                        msg.MarkProcessed(); // (ProcessedAt = UtcNow)
                    }
                    catch (Exception e)
                    {
                        msg.MarkFailed(e.Message);
                    }
                }

                if (batch.Count > 0)
                    await db.SaveChangesAsync(ct);
            }
        }
    }
}
