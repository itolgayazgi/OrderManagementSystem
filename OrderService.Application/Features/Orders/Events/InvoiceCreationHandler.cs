using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using OrderService.Application.IntegrationEvents;
using OrderService.Domain.Entity;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Features.Orders.Events
{
    public class InvoiceCreationHandler : INotificationHandler<OrderCreatedNotification>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvoiceGateway _invoiceGateway;

        public InvoiceCreationHandler(IUnitOfWork unitOfWork, IInvoiceGateway invoiceGateway)
        {
            _unitOfWork = unitOfWork;
            _invoiceGateway = invoiceGateway;
        }

        public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
        {
            // Case'e göre bu işlem harici bir entegrasyon (mock servis) ile simule ediliyor.
            var (invoiceNo, externalTraceId) = await _invoiceGateway.CreateAsync(notification.OrderId, cancellationToken);

            var invoice = new Invoice(notification.OrderId, invoiceNo, externalTraceId);

            // Simülasyon gecikmesi ve durum güncellemesi
            await Task.Delay(400, cancellationToken); // appsettings'den okunabilir
            invoice.MarkCompleted();

            await _unitOfWork.InvoiceRepository.AddAsync(invoice);
        }
    }
}
