using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Interfaces;

namespace OrderService.OutboxWorker.Integrations
{
    public class HttpFakeInvoiceGateway : IInvoiceGateway
    {
        public Task<(string invoiceNo, string externalTraceId)> CreateAsync(Guid orderId, CancellationToken ct)
        {
            // Mock: gerçek hayatta burada HTTP çağrısı yaparsın
            var invoiceNo = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
            var traceId = Guid.NewGuid().ToString("N");
            return Task.FromResult((invoiceNo, traceId));
        }
    }
}
