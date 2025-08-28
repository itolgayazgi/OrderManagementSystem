using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Enum;

namespace OrderService.Domain.Entity
{
    public class Invoice : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public string InvoiceNo { get; private set; } = default!;
        public string? ExternalTraceId { get; private set; }
        public InvoiceStatus Status { get; private set; } = InvoiceStatus.Created;
        public DateTime? CompletedAt { get; set; }

        private Invoice() { }

        public Invoice(Guid orderId, string invoiceNo, string? externalTraceId)
        {
            OrderId = orderId;
            InvoiceNo = invoiceNo;
            ExternalTraceId = externalTraceId;
            Status = InvoiceStatus.Created;
        }

        /// <summary>
        /// Faturayı başarılı olarak işaretler.
        /// </summary>
        public void MarkCompleted()
        {
            if (Status == InvoiceStatus.Completed)
                return; // ikinci kez tamamlanmaya çalışırsa sessizce atla (idempotent davranış)

            if (Status == InvoiceStatus.Failed)
                throw new InvalidOperationException("Hata durumundaki bir fatura tamamlanamaz.");

            Status = InvoiceStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            Update(); // BaseEntity'deki UpdatedAt vs. varsa günceller
        }

        /// <summary>
        /// Faturayı başarısız olarak işaretler (opsiyonel)
        /// </summary>
        public void MarkFailed(string? reason = null)
        {
            if (Status == InvoiceStatus.Completed)
                throw new InvalidOperationException("Tamamlanan bir fatura başarısız olarak işaretlenemez.");

            Status = InvoiceStatus.Failed;
            CompletedAt = DateTime.UtcNow;
            // reason'ı ayrı bir alan olarak saklamak istersen property ekleyebilirsin
            Update();
        }
    }
}
