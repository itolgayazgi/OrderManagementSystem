using OrderService.Domain.Interfaces;

namespace OrderService.API
{
    public class FakeInvoiceGateway : IInvoiceGateway
    {
        public Task<(string, string)> CreateAsync(Guid orderId, CancellationToken ct)
        {
            return Task.FromResult(($"INV-{DateTime.UtcNow:yyyyMMdd}", Guid.NewGuid().ToString("N")));
        }
    }
}
