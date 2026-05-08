using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerSvc.Application.Customers.Commands;
using CustomerSvc.Application.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace CustomerSvc.HttpApi.Controllers;

/// <summary>
/// API controller for customer operations.
/// </summary>
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerController : AbpController
{
    private readonly IMediator _mediator;

    public CustomerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets the current customer's profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<CustomerProfileDto>> GetMyProfile()
    {
        var customerId = GetCurrentUserId();
        var result = await _mediator.Send(new GetCustomerProfileQuery(customerId));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets a customer by ID (admin only).
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CustomerProfileDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCustomerProfileQuery(id));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Updates the current customer's profile.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var customerId = GetCurrentUserId();
        
        await _mediator.Send(new UpdateCustomerProfileCommand(
            customerId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.ProfileImageUrl));

        return NoContent();
    }

    /// <summary>
    /// Searches customers (admin only).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResultDto<CustomerListItemDto>>> Search(
        [FromQuery] string? searchTerm,
        [FromQuery] string? loyaltyTier,
        [FromQuery] bool? isActive,
        [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 20)
    {
        var result = await _mediator.Send(new SearchCustomersQuery(
            searchTerm, loyaltyTier, isActive, skipCount, maxResultCount));

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in claims");
        
        return userId;
    }
}

/// <summary>
/// API controller for customer addresses.
/// </summary>
[ApiController]
[Route("api/customers/me/addresses")]
[Authorize]
public class CustomerAddressController : AbpController
{
    private readonly IMediator _mediator;

    public CustomerAddressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all addresses for the current customer.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DeliveryAddressDto>>> GetAddresses()
    {
        var customerId = GetCurrentUserId();
        var result = await _mediator.Send(new GetCustomerAddressesQuery(customerId));
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific address.
    /// </summary>
    [HttpGet("{addressId:guid}")]
    public async Task<ActionResult<DeliveryAddressDto>> GetAddress(Guid addressId)
    {
        var customerId = GetCurrentUserId();
        var result = await _mediator.Send(new GetDeliveryAddressQuery(customerId, addressId));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Adds a new delivery address.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> AddAddress([FromBody] AddAddressRequest request)
    {
        var customerId = GetCurrentUserId();
        
        var addressId = await _mediator.Send(new AddDeliveryAddressCommand(
            customerId,
            request.Label,
            request.Street,
            request.BuildingNumber,
            request.Floor,
            request.Apartment,
            request.City,
            request.District,
            request.PostalCode,
            request.Country,
            request.Latitude,
            request.Longitude,
            request.DeliveryInstructions,
            request.IsDefault));

        return CreatedAtAction(nameof(GetAddress), new { addressId }, addressId);
    }

    /// <summary>
    /// Updates an existing address.
    /// </summary>
    [HttpPut("{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] UpdateAddressRequest request)
    {
        var customerId = GetCurrentUserId();
        
        await _mediator.Send(new UpdateDeliveryAddressCommand(
            customerId,
            addressId,
            request.Label,
            request.Street,
            request.BuildingNumber,
            request.Floor,
            request.Apartment,
            request.City,
            request.District,
            request.PostalCode,
            request.Country,
            request.Latitude,
            request.Longitude,
            request.DeliveryInstructions));

        return NoContent();
    }

    /// <summary>
    /// Sets an address as default.
    /// </summary>
    [HttpPost("{addressId:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid addressId)
    {
        var customerId = GetCurrentUserId();
        await _mediator.Send(new SetDefaultAddressCommand(customerId, addressId));
        return NoContent();
    }

    /// <summary>
    /// Removes an address.
    /// </summary>
    [HttpDelete("{addressId:guid}")]
    public async Task<IActionResult> RemoveAddress(Guid addressId)
    {
        var customerId = GetCurrentUserId();
        await _mediator.Send(new RemoveDeliveryAddressCommand(customerId, addressId));
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in claims");
        
        return userId;
    }
}

#region Request DTOs

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfileImageUrl);

public record AddAddressRequest(
    string Label,
    string Street,
    string BuildingNumber,
    string? Floor,
    string? Apartment,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double? Latitude,
    double? Longitude,
    string? DeliveryInstructions,
    bool IsDefault = false);

public record UpdateAddressRequest(
    string Label,
    string Street,
    string BuildingNumber,
    string? Floor,
    string? Apartment,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double? Latitude,
    double? Longitude,
    string? DeliveryInstructions);

#endregion
