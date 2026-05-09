using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerSvc.Application.Customers.Commands;
using CustomerSvc.Application.Customers.Queries;
using CustomerSvc.Application.Contracts.Customers.Dtos;
using CustomerSvc.HttpApi.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace CustomerSvc.HttpApi.Controllers;

/// <summary>
/// API controller for customer payment methods.
/// </summary>
[ApiController]
[Route("api/customers/me/payment-methods")]
[Authorize]
public class PaymentMethodController : AbpController
{
    private readonly IMediator _mediator;

    public PaymentMethodController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets all payment methods for the current customer.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PaymentMethodDto>>> GetPaymentMethods()
    {
        var customerId = GetCurrentUserId();
        var result = await _mediator.Send(new GetCustomerPaymentMethodsQuery(customerId));
        return Ok(result);
    }

    /// <summary>
    /// Adds a new payment method.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> AddPaymentMethod([FromBody] AddPaymentMethodRequest request)
    {
        var customerId = GetCurrentUserId();
        
        var paymentMethodId = await _mediator.Send(new AddPaymentMethodCommand(
            customerId,
            request.Type,
            request.Label,
            request.Last4Digits,
            request.CardBrand,
            request.ExternalToken,
            request.ExpiryDate,
            request.IsDefault));

        return CreatedAtAction(nameof(GetPaymentMethods), paymentMethodId);
    }

    /// <summary>
    /// Sets a payment method as default.
    /// </summary>
    [HttpPost("{paymentMethodId:guid}/set-default")]
    public async Task<IActionResult> SetDefault(Guid paymentMethodId)
    {
        var customerId = GetCurrentUserId();
        await _mediator.Send(new SetDefaultPaymentMethodCommand(customerId, paymentMethodId));
        return NoContent();
    }

    /// <summary>
    /// Removes a payment method.
    /// </summary>
    [HttpDelete("{paymentMethodId:guid}")]
    public async Task<IActionResult> RemovePaymentMethod(Guid paymentMethodId)
    {
        var customerId = GetCurrentUserId();
        await _mediator.Send(new RemovePaymentMethodCommand(customerId, paymentMethodId));
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

/// <summary>
/// API controller for customer loyalty program.
/// </summary>
[ApiController]
[Route("api/customers/me/loyalty")]
[Authorize]
public class LoyaltyController : AbpController
{
    private readonly IMediator _mediator;

    public LoyaltyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets loyalty information for the current customer.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<LoyaltyInfoDto>> GetLoyaltyInfo()
    {
        var customerId = GetCurrentUserId();
        var result = await _mediator.Send(new GetLoyaltyInfoQuery(customerId));
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Redeems loyalty points (for an order discount).
    /// </summary>
    [HttpPost("redeem")]
    public async Task<ActionResult<RedeemPointsResponse>> RedeemPoints([FromBody] RedeemPointsRequest request)
    {
        var customerId = GetCurrentUserId();
        
        var success = await _mediator.Send(new DeductLoyaltyPointsCommand(
            customerId,
            request.Points,
            request.OrderId));

        return Ok(new RedeemPointsResponse(success, success ? null : "Insufficient points"));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in claims");
        
        return userId;
    }
}
