using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace OrderingSvc.Domain.Orders;

/// <summary>
/// Repository interface for Order aggregate.
/// </summary>
public interface IOrderRepository : IRepository<Order, Guid>
{
    /// <summary>
    /// Gets an order with all items loaded.
    /// </summary>
    Task<Order?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by customer ID.
    /// </summary>
    Task<List<Order>> GetByCustomerIdAsync(
        Guid customerId,
        int skipCount = 0,
        int maxResultCount = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by restaurant ID.
    /// </summary>
    Task<List<Order>> GetByRestaurantIdAsync(
        Guid restaurantId,
        OrderStatus? status = null,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders assigned to a rider.
    /// </summary>
    Task<List<Order>> GetByRiderIdAsync(
        Guid riderId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders by status.
    /// </summary>
    Task<List<Order>> GetByStatusAsync(
        OrderStatus status,
        int skipCount = 0,
        int maxResultCount = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique order number.
    /// </summary>
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
}
