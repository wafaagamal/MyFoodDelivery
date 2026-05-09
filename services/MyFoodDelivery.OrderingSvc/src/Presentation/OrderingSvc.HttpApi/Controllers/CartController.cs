using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderingSvc.Application.Contracts.Carts;
using OrderingSvc.Application.Contracts.Carts.Dtos;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace OrderingSvc.HttpApi.Controllers;

/// <summary>
/// REST API controller for cart management.
/// Uses ABP Application Services.
/// </summary>
[RemoteService]
[Area("ordering")]
[Route("api/cart")]
[Authorize]
public class CartController : AbpControllerBase
{
    private readonly ICartAppService _cartAppService;

    public CartController(ICartAppService cartAppService)
    {
        _cartAppService = cartAppService;
    }

    /// <summary>
    /// Get current user's cart
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CartDto>> GetAsync()
    {
        var result = await _cartAppService.GetAsync();
        return result != null ? Ok(result) : NoContent();
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItemAsync([FromBody] AddToCartDto input)
    {
        var result = await _cartAppService.AddItemAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items")]
    public async Task<ActionResult<CartDto>> UpdateItemAsync([FromBody] UpdateCartItemDto input)
    {
        var result = await _cartAppService.UpdateItemAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{menuItemId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveItemAsync(Guid menuItemId)
    {
        var result = await _cartAppService.RemoveItemAsync(new RemoveFromCartDto { MenuItemId = menuItemId });
        return Ok(result);
    }

    /// <summary>
    /// Clear cart
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> ClearAsync()
    {
        await _cartAppService.ClearAsync();
        return NoContent();
    }

    /// <summary>
    /// Apply promo code
    /// </summary>
    [HttpPost("promo-code")]
    public async Task<ActionResult<CartDto>> ApplyPromoCodeAsync([FromBody] ApplyPromoCodeDto input)
    {
        var result = await _cartAppService.ApplyPromoCodeAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Remove promo code
    /// </summary>
    [HttpDelete("promo-code")]
    public async Task<ActionResult<CartDto>> RemovePromoCodeAsync()
    {
        var result = await _cartAppService.RemovePromoCodeAsync();
        return Ok(result);
    }

    /// <summary>
    /// Set tip amount
    /// </summary>
    [HttpPut("tip")]
    public async Task<ActionResult<CartDto>> SetTipAsync([FromBody] SetTipDto input)
    {
        var result = await _cartAppService.SetTipAsync(input);
        return Ok(result);
    }
}
