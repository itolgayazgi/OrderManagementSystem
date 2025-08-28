using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;

namespace OrderService.API.Controllers
{
    /// <summary>Products read-only endpoints.</summary>
    [ApiController]
    [Route("api/[Controller]")]
    public class ProductController : ControllerBase
    {
        private readonly OrderDbContext _db;
        public ProductController(OrderDbContext db) => _db = db;

        /// <summary>Returns all products.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList() => Ok(await _db.Product.AsNoTracking().ToListAsync());

        /// <summary>Returns a product by id.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var p = await _db.Product.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Sku, x.Name, x.Price, x.StockQuantity })
                .FirstOrDefaultAsync(ct);

            return p is null ? NotFound() : Ok(p);
        }
    }
}
