using DeliverySvc.Application.Contracts.DeliveryTasks;
using DeliverySvc.Application.Contracts.DeliveryTasks.Dtos;
using DeliverySvc.Application.DeliveryTasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace DeliverySvc.HttpApi.Controllers;

[ApiController]
[Route("api/delivery-tasks")]
[Authorize]
public class DeliveryTaskController : AbpController
{
    private readonly DeliveryTaskAppService _deliveryTaskAppService;

    public DeliveryTaskController(DeliveryTaskAppService deliveryTaskAppService)
    {
        _deliveryTaskAppService = deliveryTaskAppService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeliveryTaskDto>> GetAsync(Guid id)
    {
        var result = await _deliveryTaskAppService.GetAsync(id);
        return Ok(result);
    }

    [HttpGet("by-order/{orderId:guid}")]
    public async Task<ActionResult<DeliveryTaskDto>> GetByOrderAsync(Guid orderId)
    {
        var result = await _deliveryTaskAppService.GetByOrderAsync(orderId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryTaskDto>> CreateAsync([FromBody] CreateDeliveryTaskDto input)
    {
        var result = await _deliveryTaskAppService.CreateAsync(input);
        return CreatedAtAction(nameof(GetAsync), new { id = result.Id }, result);
    }

    [HttpGet("available")]
    public async Task<ActionResult<List<DeliveryTaskDto>>> GetAvailableAsync()
    {
        var result = await _deliveryTaskAppService.GetAvailableAsync();
        return Ok(result);
    }

    [HttpGet("my/active")]
    public async Task<ActionResult<DeliveryTaskDto?>> GetMyActiveAsync()
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        var result = await _deliveryTaskAppService.GetActiveForRiderAsync(userId);
        if (result == null) return NoContent();
        return Ok(result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<DeliveryTaskDto>> AcceptAsync(Guid id)
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        var result = await _deliveryTaskAppService.AcceptAsync(id, userId);
        return Ok(result);
    }

    [HttpPost("{id:guid}/decline")]
    public IActionResult DeclineAsync(Guid id)
    {
        // Decline means the rider dismisses the available task without accepting.
        // No state change on the task — it stays Pending for other riders.
        return NoContent();
    }

    [HttpGet("my")]
    public async Task<ActionResult<object>> GetMyTasksAsync([FromQuery] int skipCount = 0, [FromQuery] int maxResultCount = 50)
    {
        var userId = CurrentUser.Id ?? throw new UnauthorizedAccessException();
        var result = await _deliveryTaskAppService.GetMyTasksAsync(userId, skipCount, maxResultCount);
        return Ok(new { items = result });
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<DeliveryTaskDto>> AssignRiderAsync(Guid id, [FromBody] AssignRiderDto input)
    {
        var result = await _deliveryTaskAppService.AssignRiderAsync(id, input);
        return Ok(result);
    }

    [HttpPost("{id:guid}/picked-up")]
    public async Task<IActionResult> MarkPickedUpAsync(Guid id)
    {
        await _deliveryTaskAppService.MarkPickedUpAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/delivered")]
    public async Task<IActionResult> MarkDeliveredAsync(Guid id)
    {
        await _deliveryTaskAppService.MarkDeliveredAsync(id);
        return NoContent();
    }
}
