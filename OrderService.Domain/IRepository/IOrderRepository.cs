using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Entity;

namespace OrderService.Domain.IRepository
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<Order>> GetAllWithItemsAsync(CancellationToken ct);
    }
}
