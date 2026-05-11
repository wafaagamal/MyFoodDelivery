using DeliverySvc.Application.Contracts.Riders;
using DeliverySvc.Application.Contracts.Riders.Dtos;
using DeliverySvc.Application.Riders;
using DeliverySvc.Domain.Riders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliverySvc.HttpApi.Controllers;

[ApiController]
[Route("api/riders")]
[Authorize]
public class RiderController : AbpController
{
    private readonly RiderAppService _riderAppService;

    public RiderController(RiderAppService riderAppService)
    {
        _riderAppService = riderAppService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<RiderDto>> GetMyAsync()
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        try
        {
            var result = await _riderAppService.GetAsync(userId);
            return Ok(result);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost("my")]
    public async Task<ActionResult<RiderDto>> CreateMyAsync()
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        var result = await _riderAppService.CreateAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RiderDto>> GetAsync(Guid id)
    {
        var result = await _riderAppService.GetAsync(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<RiderListDto>>> GetListAsync([FromQuery] bool? isActive)
    {
        var result = await _riderAppService.GetListAsync(isActive);
        return Ok(result);
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<RiderListDto>>> GetAvailableAsync()
    {
        var result = await _riderAppService.GetAvailableRidersAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RiderDto>> CreateAsync([FromQuery] Guid userId)
    {
        var result = await _riderAppService.CreateAsync(userId);
        return CreatedAtAction(nameof(GetAsync), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/vehicle")]
    public async Task<ActionResult<RiderDto>> UpdateVehicleAsync(Guid id, [FromBody] UpdateRiderVehicleDto input)
    {
        var result = await _riderAppService.UpdateVehicleAsync(id, input);
        return Ok(result);
    }

    [HttpGet("my/today-summary")]
    public async Task<ActionResult<object>> GetMyTodaySummaryAsync()
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        try
        {
            var rider = await _riderAppService.GetAsync(userId);
            return Ok(new { deliveries = rider.TotalDeliveries, earnings = rider.TotalEarnings, hours = 0, tips = 0 });
        }
        catch
        {
            return Ok(new { deliveries = 0, earnings = 0, hours = 0, tips = 0 });
        }
    }

    [HttpGet("my/earnings")]
    public async Task<ActionResult<object>> GetMyEarningsAsync([FromQuery] string period = "week")
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        try
        {
            var rider = await _riderAppService.GetAsync(userId);
            var avgPerDelivery = rider.TotalDeliveries > 0 ? (double)rider.TotalEarnings / rider.TotalDeliveries : 0;
            return Ok(new
            {
                totalEarnings = rider.TotalEarnings,
                totalDeliveries = rider.TotalDeliveries,
                totalTips = 0,
                activeHours = 0,
                avgPerDelivery,
                availableBalance = rider.TotalEarnings
            });
        }
        catch
        {
            return Ok(new { totalEarnings = 0, totalDeliveries = 0, totalTips = 0, activeHours = 0, avgPerDelivery = 0, availableBalance = 0 });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, [FromBody] UpdateRiderOnlineStatusDto input)
    {
        var status = input.IsOnline ? RiderStatus.Available : RiderStatus.Offline;
        await _riderAppService.UpdateStatusAsync(id, status);
        return NoContent();
    }

    [HttpPatch("{id:guid}/location")]
    public async Task<IActionResult> UpdateLocationAsync(Guid id, [FromBody] UpdateRiderLocationDto input)
    {
        await _riderAppService.UpdateLocationAsync(id, input);
        return NoContent();
    }

    [HttpGet("my/earnings/daily")]
    public ActionResult<object> GetMyDailyEarningsAsync([FromQuery] string period = "week")
    {
        // Return empty daily breakdown — detailed tracking not yet implemented
        return Ok(Array.Empty<object>());
    }

    [HttpGet("my/earnings/transactions")]
    public ActionResult<object> GetMyTransactionsAsync([FromQuery] string period = "week")
    {
        // Return empty transactions — detailed tracking not yet implemented
        return Ok(Array.Empty<object>());
    }

    [HttpPut("{id:guid}")]
    public IActionResult UpdateProfileAsync(Guid id, [FromBody] object input)
    {
        // Stub — profile updates not yet persisted
        return NoContent();
    }

    [HttpPut("{id:guid}/settings")]
    public IActionResult UpdateSettingsAsync(Guid id, [FromBody] object input)
    {
        // Stub — settings updates not yet persisted
        return NoContent();
    }

    [HttpGet("{id:guid}/settings")]
    public ActionResult<object> GetSettingsAsync(Guid id)
    {
        return Ok(new
        {
            pushNotifications = true,
            emailNotifications = true,
            soundAlerts = true,
            navigationApp = "Google Maps"
        });
    }
}
