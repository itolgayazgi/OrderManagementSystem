using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entity
{
    public class InboxMessage : BaseEntity
    {
        public string MessageId { get; private set; } = default!;
        public DateTime ProcessedAt { get; private set; } = DateTime.UtcNow;
        public string EventType { get; set; } = default!;

        private InboxMessage() { }
        public InboxMessage(string messageId, string eventType)
        {
            MessageId = messageId;
            EventType = eventType;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
