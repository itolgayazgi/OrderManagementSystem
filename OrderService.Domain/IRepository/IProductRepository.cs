using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Entity;

namespace OrderService.Domain.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetByIdsAsync(List<Guid> ids);
    }
}
