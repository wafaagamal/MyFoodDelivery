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
    [Authorize(Roles = "Admin")]
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

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, [FromBody] RiderStatus status)
    {
        await _riderAppService.UpdateStatusAsync(id, status);
        return NoContent();
    }

    [HttpPatch("{id:guid}/location")]
    public async Task<IActionResult> UpdateLocationAsync(Guid id, [FromBody] UpdateRiderLocationDto input)
    {
        await _riderAppService.UpdateLocationAsync(id, input);
        return NoContent();
    }
}
