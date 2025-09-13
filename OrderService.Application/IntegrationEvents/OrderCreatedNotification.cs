using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace OrderService.Application.IntegrationEvents
{
    // MediatR'ın bu sınıfın bir bildirim/olay olduğunu anlaması için INotification'ı implement eder.
    public class OrderCreatedNotification : INotification
    {
        // init; C# 9.0 ve üzeri versiyonlarda bu property'nin sadece nesne oluşturulurken atanabileceğini belirtir.
        // Bu, event nesnesinin sonradan değiştirilemez (immutable) olmasını sağlar, ki bu çok iyi bir pratiktir.
        public Guid OrderId { get; init; }
        public int CustomerId { get; init; }
        public decimal TotalAmount { get; init; }
        public List<OrderCreatedItemDto> Items { get; init; } = new();

        public OrderCreatedNotification(Guid orderId, int customerId, decimal totalAmount, List<OrderCreatedItemDto> items)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
            Items = items;
        }
    }

    public class OrderCreatedItemDto
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
