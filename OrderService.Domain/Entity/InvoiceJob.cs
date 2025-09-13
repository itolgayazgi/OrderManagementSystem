using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Enum;

namespace OrderService.Domain.Entity
{
    public class InvoiceJob : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public JobStatus Status { get; private set; } = JobStatus.Pending;
        public int AttemptCount { get; private set; }
        public DateTime? NextAttemptAt { get; private set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; private set; }
        public string PayloadJson { get; private set; } = "{}";

        private InvoiceJob() { }

        public InvoiceJob(Guid orderId, string payloadJson)
        {
            OrderId = orderId;
            PayloadJson = payloadJson;
        }

        public void MarkScheduled(TimeSpan delay)
        {
            NextAttemptAt = DateTime.UtcNow.Add(delay);
        }

        public void MarkProcessed()
        {
            Status = JobStatus.Processed;
            ProcessedAt = DateTime.UtcNow;    
        }

        public void MarkFailedAndBackoff(TimeSpan backoff)
        {
            Status = JobStatus.Pending;
            AttemptCount++;
            NextAttemptAt = DateTime.UtcNow.Add(backoff);
        }
    }
}
