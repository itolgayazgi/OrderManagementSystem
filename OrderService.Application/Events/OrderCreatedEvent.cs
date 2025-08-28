using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Events
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; init; }
        public int CustomerId { get; init; }
        public decimal TotalAmount { get; init; }
        public List<OrderCreatedItem> Items { get; init; } = new();
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public string Version { get; init; } = "1";

    }

    public class OrderCreatedItem
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
