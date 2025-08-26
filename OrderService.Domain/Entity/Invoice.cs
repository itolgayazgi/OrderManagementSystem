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

        private Invoice() { }

        public Invoice(Guid orderId, string invoiceNo, string? externalTraceId)
        {
            OrderId = orderId;
            InvoiceNo = invoiceNo;
            ExternalTraceId = externalTraceId;
        }
    }
}
