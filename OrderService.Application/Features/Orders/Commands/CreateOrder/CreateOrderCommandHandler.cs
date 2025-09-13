using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using OrderService.Domain.Entity;
using OrderService.Domain.Interfaces;
using OrderService.Domain.IRepository;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder
{

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var productIds = request.Items.Select(item => item.ProductId).ToList();
            // Ürünleri sadece fiyat ve varlık kontrolü için çekiyoruz.
            var products = await _unitOfWork.ProductRepository.GetByIdsAsync(productIds);

            var newOrder = new Order(request.CustomerId);

            foreach (var itemDto in request.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                if (product == null)
                    throw new Exception($"Product not found with Id: {itemDto.ProductId}");

                newOrder.AddItem(product.Id, itemDto.Quantity, product.Price);
            }

            await _unitOfWork.OrderRepository.AddAsync(newOrder);

            // Outbox pattern ile event'i kaydetme işlemi doğru.
            var orderCreatedEvent = new
            {
                OrderId = newOrder.Id,
                CustomerId = newOrder.CustomerId,
                TotalAmount = newOrder.TotalAmount,
                Items = newOrder.Items.Select(i => new { i.ProductId, i.Quantity, i.UnitPrice }).ToList()
            };

            var payload = JsonSerializer.Serialize(orderCreatedEvent);
            var outboxMessage = new OutboxMessage("OrderCreated", payload, DateTime.UtcNow, null);

            await _unitOfWork.OutboxMessageRepository.AddAsync(outboxMessage);
            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return newOrder.Id;
        }
    }
}
