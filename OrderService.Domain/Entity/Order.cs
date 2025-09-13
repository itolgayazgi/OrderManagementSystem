using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Enum;

namespace OrderService.Domain.Entity
{
    public class Order : BaseEntity
    {
        public int CustomerId { get; set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public decimal TotalAmount { get; private set; }

        private readonly List<OrderItem> _items = new();
        public IReadOnlyCollection<OrderItem> Items => _items;

        private Order() { }

        public Order(int customerId) //TotalAmount'u burda vermememizin sebebi, sipariş kalemleri eklenirken hesaplandığından.
        {
            CustomerId = customerId;
        }

        public void AddItem(Guid productId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            // Bu çağrı artık daha temiz.
            var item = new OrderItem(this, productId, quantity, unitPrice);

            _items.Add(item);
            TotalAmount += item.LineTotal;
            Update();
        }

        public void MarkCompleted()
        {
            Status = OrderStatus.Completed;
            Update();
        }

        public void MarkFailed()
        {
            Status = OrderStatus.Failed;
            Update();

        }
    }
}
