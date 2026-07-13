using ISphereHub.Application.Interfaces;
using ISphereHub.BuildingBlocks.Dtos.OrderDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISphereHub.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<OrderDto>> CreateOnlineOrder([FromBody] CreateOnlineOrderDto dto, CancellationToken ct)
        {
            var result = await _orderService.CreateOnlineOrderAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("in-store")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> CreateInStoreSale([FromBody] CreateInStoreSaleDto dto, CancellationToken ct)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var adminUsername = User.FindFirstValue(ClaimTypes.Name) ?? "staff";

            var result = await _orderService.CreateInStoreSaleAsync(dto, adminId, adminUsername, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<OrderListResponse>> GetAll([FromQuery] OrderQueryDto query, CancellationToken ct)
        {
            var result = await _orderService.GetAllOrdersAsync(query, ct);
            return Ok(result);
        }

        [HttpGet("stats")]
        [Authorize]
        public async Task<ActionResult<OrderStatsSummaryDto>> GetStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken ct)
        {
            var result = await _orderService.GetStatsSummaryAsync(fromDate, toDate, ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> GetById(string id, CancellationToken ct)
        {
            var result = await _orderService.GetOrderByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<ActionResult<OrderDto>> UpdateStatus(string id, [FromBody] UpdateOrderStatusDto dto, CancellationToken ct)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, dto, ct);
            return Ok(result);
        }
    }
}
