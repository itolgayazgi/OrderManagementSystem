using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                 ?? "Host=127.0.0.1;Port=5432;Database=OrderInvoiceDb;Username=postgres;Password=password";
            var builder = new DbContextOptionsBuilder<OrderDbContext>()
                .UseNpgsql(cs, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            return new OrderDbContext(builder.Options);
        }
    }
}
