using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Interfaces;
using OrderService.Domain.IRepository;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;
        public IOrderRepository OrderRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public IInvoiceRepository InvoiceRepository { get; private set; }
        public IOutboxMessageRepository OutboxMessageRepository { get; private set; }

        public UnitOfWork(OrderDbContext context)
        {
            _context = context;
            OrderRepository = new OrderRepository(_context);
            ProductRepository = new ProductRepository(_context);
            InvoiceRepository = new InvoiceRepository(_context);
            OutboxMessageRepository = new OutboxMessageRepository(_context);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
