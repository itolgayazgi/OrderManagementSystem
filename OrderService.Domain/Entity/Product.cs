using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Entity
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; } = default!;
        public string Sku { get; private set; } = default!; //sebebi?
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }

        private Product() { }

        public Product(string sku, string name, decimal price, int stockQuantity) 
        {
            Sku = sku;
            Name = name;
            Price = price;
            StockQuantity = stockQuantity;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (StockQuantity < quantity) throw new InvalidOperationException("Mevcut Stok Hatası");

            StockQuantity -= quantity;

            Update();
        }

    }
}
