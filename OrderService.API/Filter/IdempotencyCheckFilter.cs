using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using OrderService.Domain.Entity;
using OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace OrderService.API.Filter
{
    public class IdempotencyCheckFilter : IAsyncActionFilter
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<IdempotencyCheckFilter> _logger;

        public IdempotencyCheckFilter(OrderDbContext context, ILogger<IdempotencyCheckFilter> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. İstek başlığından (header) Idempotency-Key'i al.
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKeyHeader))
            {
                context.Result = new BadRequestObjectResult("X-Idempotency-Key header is missing.");
                return;
            }

            var key = idempotencyKeyHeader.ToString();

            // 2. Bu key daha önce veritabanına kaydedilmiş mi diye kontrol et.
            var existingKey = await _context.IdempotencyKey.FirstOrDefaultAsync(k => k.Key == key);

            if (existingKey != null)
            {
                // 3. Eğer key varsa ve yanıtı belliyse, bu mükerrer bir istektir.
                if (existingKey.ResponseStatusCode > 0)
                {
                    _logger.LogInformation("Returning stored response for idempotency key: {Key}", key);
                    var storedResponse = new ObjectResult(JsonSerializer.Deserialize<object>(existingKey.ResponseBody))
                    {
                        StatusCode = existingKey.ResponseStatusCode
                    };
                    context.Result = storedResponse;
                    return;
                }
                // Eğer key var ama yanıtı henüz belli değilse (işlem devam ediyor),
                // bir çakışma olduğunu belirtmek için bir hata dönebiliriz.
                context.Result = new ConflictObjectResult("Request with this key is already being processed.");
                return;
            }

            // 4. Eğer key yoksa, bu yeni bir istektir.
            var newKey = new IdempotencyKey(key, context.ActionDescriptor.DisplayName!, ""); // Request hash eklenebilir
            await _context.IdempotencyKey.AddAsync(newKey);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New idempotency key registered: {Key}", key);

            // 5. Controller'daki asıl metodu (CreateOrder) çalıştır.
            var executedContext = await next();

            // 6. Metot çalıştıktan sonra, oluşan sonucu (result) yakala.
            if (executedContext.Result is ObjectResult result)
            {
                // 7. Yanıtı veritabanındaki key kaydına işle ve tekrar kaydet.
                var responseBody = JsonSerializer.Serialize(result.Value);
                newKey.SetResponse(result.StatusCode ?? 500, responseBody);
                _context.IdempotencyKey.Update(newKey);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Stored response for idempotency key: {Key}", key);
            }
        }
    }
}
