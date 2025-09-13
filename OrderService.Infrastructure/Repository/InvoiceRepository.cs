using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Entity;
using OrderService.Domain.IRepository;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repository
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(OrderDbContext context) : base(context)
        {
        }
    }
}
