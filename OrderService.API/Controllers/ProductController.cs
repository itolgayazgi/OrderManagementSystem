using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Persistence;

namespace OrderService.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ProductController : ControllerBase
    {
        private readonly OrderDbContext _db;
        public ProductController(OrderDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetList() => Ok(await _db.Product.AsNoTracking().ToListAsync());
    } 
}
