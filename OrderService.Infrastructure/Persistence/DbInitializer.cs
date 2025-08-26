using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entity;

namespace OrderService.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(OrderDbContext db)
        {
            // migrasyon yoksa ya da DB yeni ise tablo oluşturulduğundan eminiz
            await db.Database.EnsureCreatedAsync();

            if (!await db.Product.AnyAsync())
            {
                var products = new List<Product>
                {
                   new Product("SKU-001", "MacBook Pro 14", 52000m, 10),
                   new Product("SKU-002", "Logitech MX Keys", 4500m, 25),
                   new Product("SKU-003", "Dell 27\" Monitor", 9800m, 12),
                   new Product("SKU-004", "AirPods Pro", 8200m, 30)
                };

                await db.Product.AddRangeAsync(products);
                await db.SaveChangesAsync();
            }
        }
    }
}
