using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using OrderingSvc.Application.Contracts.Orders.Dtos;

namespace OrderingSvc.Application.Contracts.Orders;

/// <summary>
/// ABP Application Service interface for order management (Customer-facing).
/// </summary>
public interface IOrderAppService : IApplicationService
{
    #region Order CRUD

    /// <summary>
    /// Get order by ID
    /// </summary>
    Task<OrderDto> GetAsync(Guid id);

    /// <summary>
    /// Get order by order number
    /// </summary>
    Task<OrderDto> GetByNumberAsync(string orderNumber);

    /// <summary>
    /// Get customer's orders (paginated)
    /// </summary>
    Task<PagedResultDto<OrderListDto>> GetListAsync(GetOrderListInput input);

    /// <summary>
    /// Create order from current cart
    /// </summary>
    Task<OrderDto> CreateAsync(CreateOrderDto input);

    /// <summary>
    /// Cancel an order
    /// </summary>
    Task CancelAsync(Guid id, CancelOrderDto input);

    #endregion

    #region Tracking

    /// <summary>
    /// Get real-time order tracking info
    /// </summary>
    Task<OrderTrackingDto> GetTrackingAsync(Guid id);

    #endregion

    #region Rating

    /// <summary>
    /// Rate a completed order
    /// </summary>
    Task RateAsync(RateOrderDto input);

    #endregion
}

/// <summary>
/// ABP Application Service interface for restaurant order management.
/// </summary>
public interface IRestaurantOrderAppService : IApplicationService
{
    /// <summary>
    /// Get orders for a restaurant
    /// </summary>
    Task<PagedResultDto<OrderListDto>> GetListAsync(GetRestaurantOrdersInput input);

    /// <summary>
    /// Get order details
    /// </summary>
    Task<OrderDto> GetAsync(Guid restaurantId, Guid orderId);

    /// <summary>
    /// Accept an incoming order
    /// </summary>
    Task AcceptAsync(Guid restaurantId, Guid orderId, int preparationMinutes);

    /// <summary>
    /// Reject an order
    /// </summary>
    Task RejectAsync(Guid restaurantId, Guid orderId, string reason);

    /// <summary>
    /// Mark order as being prepared
    /// </summary>
    Task StartPreparingAsync(Guid restaurantId, Guid orderId);

    /// <summary>
    /// Mark order as ready for pickup
    /// </summary>
    Task MarkReadyAsync(Guid restaurantId, Guid orderId);
}
