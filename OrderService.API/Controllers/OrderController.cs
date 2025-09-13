using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Filter;
using OrderService.Application.Features.Orders.Commands.CreateOrder;
using OrderService.Application.Features.Orders.Queries;
using OrderService.Application.Features.Products.Queries;

namespace OrderService.API.Controllers
{
    /// <summary>Order endpoints (idempotent create).</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IMediator mediator, ILogger<OrderController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new order with idempotency support
        /// </summary>
        /// <param name="command">Order creation command</param>
        /// <returns>Created order ID</returns>
        /// <response code="200">Order created successfully</response>
        /// <response code="400">Invalid request data or missing X-Idempotency-Key header</response>
        /// <response code="409">Request with this idempotency key is already being processed</response>
        /// <response code="500">Internal server error</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Order
        ///     Headers: X-Idempotency-Key: 123e4567-e89b-12d3-a456-426614174000
        ///     {
        ///         "customerId": "123e4567-e89b-12d3-a456-426614174000",
        ///         "items": [
        ///             {
        ///                 "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///                 "quantity": 2,
        ///                 "unitPrice": 15.99
        ///             }
        ///         ]
        ///     }
        /// </remarks>
        [HttpPost("CreateOrder")]
        [ServiceFilter(typeof(IdempotencyCheckFilter))]
        [ProducesResponseType(typeof(CreateOrderResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateOrderCommand command,
            [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for CreateOrder request");
                return BadRequest(ModelState);
            }

            try
            {
                var newOrderId = await _mediator.Send(command);

                _logger.LogInformation("Order created successfully with ID: {OrderId}", newOrderId);

                return Ok(new CreateOrderResponse
                {
                    OrderId = newOrderId,
                    Message = "Order created successfully"
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error while creating order: {Error}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Business logic error while creating order: {Error}", ex.Message);
                return UnprocessableEntity(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating order");
                return StatusCode(500, "An unexpected error occurred while processing your request");
            }
        }

        [HttpGet("GetList")]
        [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetList(CancellationToken cancellationToken)
        {
            var orders = await _mediator.Send(new GetAllOrdersQuery(), cancellationToken);
            return Ok(orders);
        }
    }

    /// <summary>
    /// Response model for order creation
    /// </summary>
    public class CreateOrderResponse
    {
        /// <summary>The unique identifier of the created order</summary>
        public Guid OrderId { get; set; }

        /// <summary>Success message</summary>
        public string Message { get; set; } = string.Empty;
    }
}