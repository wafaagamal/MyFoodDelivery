using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderingSvc.Application.Contracts.Orders;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace OrderingSvc.HttpApi.Controllers;

/// <summary>
/// REST API controller for customer order management.
/// Uses ABP Application Services.
/// </summary>
[RemoteService]
[Area("ordering")]
[Route("api/orders")]
[Authorize]
public class OrdersController : AbpControllerBase
{
    private readonly IOrderAppService _orderAppService;

    public OrdersController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetAsync(Guid id)
    {
        var result = await _orderAppService.GetAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    [HttpGet("by-number/{orderNumber}")]
    public async Task<ActionResult<OrderDto>> GetByNumberAsync(string orderNumber)
    {
        var result = await _orderAppService.GetByNumberAsync(orderNumber);
        return Ok(result);
    }

    /// <summary>
    /// Get customer's orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrderListDto>>> GetListAsync(
        [FromQuery] GetOrderListInput input)
    {
        var result = await _orderAppService.GetListAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Create order from cart
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateAsync([FromBody] CreateOrderDto input)
    {
        var result = await _orderAppService.CreateAsync(input);
        return CreatedAtAction(nameof(GetAsync), new { id = result.Id }, result);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> CancelAsync(Guid id, [FromBody] CancelOrderDto input)
    {
        await _orderAppService.CancelAsync(id, input);
        return Ok();
    }

    /// <summary>
    /// Get order tracking info
    /// </summary>
    [HttpGet("{id:guid}/tracking")]
    public async Task<ActionResult<OrderTrackingDto>> GetTrackingAsync(Guid id)
    {
        var result = await _orderAppService.GetTrackingAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Rate a completed order
    /// </summary>
    [HttpPost("rate")]
    public async Task<ActionResult> RateAsync([FromBody] RateOrderDto input)
    {
        await _orderAppService.RateAsync(input);
        return Ok();
    }
}
