using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entity;
using OrderService.Domain.IRepository;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(OrderDbContext context) : base(context)
        {
        }

        public Task<List<Product>> GetByIdsAsync(List<Guid> ids)
        {
            return _context.Product.Where(p => ids.Contains(p.Id)).ToListAsync();
        }
    }
}
