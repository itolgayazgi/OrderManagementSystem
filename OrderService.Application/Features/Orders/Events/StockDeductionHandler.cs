using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using OrderService.Application.IntegrationEvents;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Features.Orders.Events
{
    // INotificationHandler<T>, hangi olayı dinleyeceğini belirtir.
    public class StockDeductionHandler : INotificationHandler<OrderCreatedNotification>
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockDeductionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
        {
            // Olaydan gelen sipariş ID'si ile siparişi ve kalemlerini bul.
            // Not: Order entity'sinde Items koleksiyonu 'virtual' olmalı veya Include ile çekilmeli.
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(notification.OrderId);

            if (order == null)
            {
                // Loglama yapılabilir. Sipariş bulunamadı.
                return;
            }

            var productIds = order.Items.Select(item => item.ProductId).ToList();
            var products = await _unitOfWork.ProductRepository.GetByIdsAsync(productIds);

            try
            {
                foreach (var item in order.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null)
                    {
                        // Bu durum normalde olmamalı çünkü CreateOrder'da kontrol edildi. Ama yine de bir güvence.
                        throw new InvalidOperationException($"Product {item.ProductId} not found for order {order.Id}");
                    }

                    product.DecreaseStock(item.Quantity);
                }

                // Her şey yolunda gittiyse, sipariş durumunu "Tamamlandı" olarak güncelle.
                order.MarkCompleted();
            }
            catch (InvalidOperationException ex) // Örneğin, "Mevcut Stok Hatası"
            {
                order.MarkFailed();
            }
            finally
            {
                //OrderCreatedConsumer üzerinden bu işlem yapılacak.
                //await _unitOfWork.SaveChangesAsync(cancellationToken); 
            }
        }
    }
}
