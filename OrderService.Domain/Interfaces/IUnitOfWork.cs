using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.IRepository;

namespace OrderService.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository OrderRepository { get; }
        IProductRepository ProductRepository { get; }
        IInvoiceRepository InvoiceRepository { get; }
        IOutboxMessageRepository OutboxMessageRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
