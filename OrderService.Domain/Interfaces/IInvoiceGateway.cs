using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Interfaces
{
    public interface IInvoiceGateway
    {
        Task<(string invoiceNo, string externalTraceId)> CreateAsync(Guid orderId, CancellationToken ct);
    }
}
