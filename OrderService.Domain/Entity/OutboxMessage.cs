using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entity
{
    public class OutboxMessage : BaseEntity
    {
        public string EventType { get; private set; } = default!;   // "order.created"
        public string Payload { get; private set; } = default!;     // JSON
        public DateTime? ProcessedAt { get; private set; }
        public int RetryCount { get; private set; } = 0;
        public string? Error { get; private set; }

        // İsteğe bağlı zamanlı/ertelemeli gönderim için:
        public DateTime? ProcessAt { get; private set; }

        private OutboxMessage() { }
        public OutboxMessage(string eventType, string payload, DateTime? createdAt ,DateTime? processAt)
        {
            EventType = eventType;
            Payload = payload;
            CreatedAt = createdAt ?? DateTime.UtcNow;
            ProcessAt = processAt;
        }

        public void MarkProcessed()
        {
            ProcessedAt = DateTime.UtcNow;
            Error = null;
            Update();
        }

        public void MarkFailed(string error)
        {
            RetryCount++;
            Error = error;
            var delay = Math.Min(RetryCount * 5, 60);
            ProcessAt = DateTime.UtcNow.AddSeconds(delay);
            Update();
        }
    }
}
