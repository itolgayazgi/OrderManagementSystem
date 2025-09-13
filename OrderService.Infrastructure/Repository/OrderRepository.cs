using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entity;
using OrderService.Domain.IRepository;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repository
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(OrderDbContext context) : base(context)
        {
        }

        public async Task<List<Order>> GetAllWithItemsAsync(CancellationToken ct)
        {
            return await _context.Order
                                 .Include(o => o.Items)
                                 .AsNoTracking()
                                 .ToListAsync();
        }
    }
}
