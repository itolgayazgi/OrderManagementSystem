using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OrderService.Domain.Entity
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal { get; private set; }
        [JsonIgnore]
        public Order Order { get; private set; }

        public OrderItem() { }

        public OrderItem(Order order, Guid productId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            Order = order ?? throw new ArgumentNullException(nameof(order)); // İlişkiyi nesne referansıyla kuruyoruz.
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            LineTotal = unitPrice * quantity;

            // OrderId'yi burada set etmeye GEREK YOK.
            // OrderId = order.Id; // BU SATIRI SİLİN!
        }
    }
}
