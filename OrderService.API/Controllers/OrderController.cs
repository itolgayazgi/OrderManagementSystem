using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Events;
using OrderService.Application.Models.Order;
using OrderService.Domain.Entity;
using OrderService.Infrastructure.Persistence;

namespace OrderService.API.Controllers
{
    /// <summary>Order endpoints (idempotent create).</summary>
    [ApiController]
    [Route("api/[Controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _db;
        public OrderController(OrderDbContext db) => _db = db;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>Create order (idempotent via Idempotency-Key header)</summary>
        /// <remarks>Send header <c>Idempotency-Key: &lt;guid&gt;</c></remarks>
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,CancellationToken ct)
        {
            //Idempotency-Key
            var key = Request.Headers["Idempotency-Key"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest("Idempotency-Key header is required.");

            //Endpoint & RequestHash
            var endpoint = $"{Request.Method} {Request.Path.Value!.ToLowerInvariant()}";
            var reqJson = JsonSerializer.Serialize(request, JsonOpts);
            var reqHash = ComputeHash(reqJson); // senin yardımcı metodun

            // 3) Daha önce aynı key+endpoint için response saklı mı?
            var saved = await _db.IdempotencyKey
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Key == key && x.Endpoint == endpoint, ct);

            if (saved is not null && saved.ResponseStatusCode.HasValue)
            {
                // Aynı key + farklı body ise 409
                if (!string.Equals(saved.RequestHash, reqHash, StringComparison.Ordinal))
                    return Conflict(new { error = "Idempotency key reused with different request body." });

                Response.Headers["Idempotency-Replayed"] = "true";
                return new ContentResult
                {
                    StatusCode = saved.ResponseStatusCode,
                    Content = saved.ResponseBody,
                    ContentType = "application/json; charset=utf-8"
                };
            }

            //Transaction: Pending idempotency -> Order -> Outbox -> Response'u idempotency'ye yaz -> Commit
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                //Pending idempotency row (response boş)
                var idem = saved ?? new IdempotencyKey(key, endpoint, reqHash);
                if (saved is null) _db.IdempotencyKey.Add(idem);
                await _db.SaveChangesAsync(ct);

                //Order + Items
                var order = new Order(request.CustomerId);
                foreach (var i in request.Items)
                    order.AddItem(Guid.Parse(i.ProductId), i.Quantity, i.UnitPrice);

                _db.Order.Add(order);
                await _db.SaveChangesAsync(ct); // Id'ler oluşsun

                //Outbox event (aynı transaction içinde!)
                var evt = new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    TotalAmount = order.TotalAmount,
                    Items = order.Items.Select(i => new OrderCreatedItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };
                var evtJson = JsonSerializer.Serialize(evt, JsonOpts);
                _db.OutboxMessage.Add(new OutboxMessage("order.created", evtJson, DateTime.UtcNow, null)); 
                await _db.SaveChangesAsync(ct);

                //Response'u hazırla + idempotency kaydına işle
                var responseObj = new { orderId = order.Id, totalAmount = order.TotalAmount, status = order.Status.ToString() };
                var responseJson = JsonSerializer.Serialize(responseObj, JsonOpts);
                idem.SetResponse(StatusCodes.Status201Created, responseJson);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                //201 Created
                return Created($"/api/order/{order.Id}", responseObj);
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync(ct);

                // Muhtemel race condition: unique (Key,Endpoint) çakıştı
                var again = await _db.IdempotencyKey
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Key == key && x.Endpoint == endpoint, ct);

                if (again is not null && again.ResponseStatusCode.HasValue)
                    return new ContentResult
                    {
                        StatusCode = again.ResponseStatusCode,
                        Content = again.ResponseBody,
                        ContentType = "application/json; charset=utf-8"
                    };

                // Response hâlâ yazılmadıysa istemciye yeniden denemesini söyle (409 veya 425 tercih edilebilir)
                return StatusCode(StatusCodes.Status409Conflict, new { error = "Request is being processed, try again." });
            }
        }

        /// <summary>Get order by id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var order = await _db.Order
                .Include(o => o.Items)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (order is null) return NotFound();

            var dto = new
            {
                order.Id,
                order.CustomerId,
                Status = order.Status.ToString(),
                order.TotalAmount,
                Items = order.Items.Select(i => new
                {
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                })
            };

            return Ok(dto);
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

    }
}
