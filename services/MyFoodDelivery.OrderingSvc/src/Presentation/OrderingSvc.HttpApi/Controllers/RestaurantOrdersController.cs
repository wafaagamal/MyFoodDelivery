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
/// REST API controller for restaurant order management.
/// Uses ABP Application Services.
/// </summary>
[RemoteService]
[Area("ordering")]
[Route("api/restaurants/{restaurantId:guid}/orders")]
[Authorize]
public class RestaurantOrdersController : AbpControllerBase
{
    private readonly IRestaurantOrderAppService _restaurantOrderAppService;

    public RestaurantOrdersController(IRestaurantOrderAppService restaurantOrderAppService)
    {
        _restaurantOrderAppService = restaurantOrderAppService;
    }

    /// <summary>
    /// Get orders for a restaurant
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<OrderListDto>>> GetListAsync(
        Guid restaurantId,
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 10)
    {
        var input = new GetRestaurantOrdersInput
        {
            RestaurantId = restaurantId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            SkipCount = skipCount,
            MaxResultCount = maxResultCount
        };
        var result = await _restaurantOrderAppService.GetListAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Get order details
    /// </summary>
    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDto>> GetAsync(Guid restaurantId, Guid orderId)
    {
        var result = await _restaurantOrderAppService.GetAsync(restaurantId, orderId);
        return Ok(result);
    }

    /// <summary>
    /// Accept an order
    /// </summary>
    [HttpPost("{orderId:guid}/accept")]
    public async Task<ActionResult> AcceptAsync(
        Guid restaurantId,
        Guid orderId,
        [FromBody] int preparationMinutes)
    {
        await _restaurantOrderAppService.AcceptAsync(restaurantId, orderId, preparationMinutes);
        return Ok();
    }

    /// <summary>
    /// Reject an order
    /// </summary>
    [HttpPost("{orderId:guid}/reject")]
    public async Task<ActionResult> RejectAsync(
        Guid restaurantId,
        Guid orderId,
        [FromBody] string reason)
    {
        await _restaurantOrderAppService.RejectAsync(restaurantId, orderId, reason);
        return Ok();
    }

    /// <summary>
    /// Start preparing an order
    /// </summary>
    [HttpPost("{orderId:guid}/start-preparing")]
    public async Task<ActionResult> StartPreparingAsync(Guid restaurantId, Guid orderId)
    {
        await _restaurantOrderAppService.StartPreparingAsync(restaurantId, orderId);
        return Ok();
    }

    /// <summary>
    /// Mark order as ready for pickup
    /// </summary>
    [HttpPost("{orderId:guid}/ready")]
    public async Task<ActionResult> MarkReadyAsync(Guid restaurantId, Guid orderId)
    {
        await _restaurantOrderAppService.MarkReadyAsync(restaurantId, orderId);
        return Ok();
    }
}
