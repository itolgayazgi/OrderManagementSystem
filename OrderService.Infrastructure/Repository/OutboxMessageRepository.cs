using OrderService.Domain.Entity;
using OrderService.Domain.IRepository;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repository
{
    public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
    {
        public OutboxMessageRepository(OrderDbContext context) : base(context)
        {
        }
    }
}
