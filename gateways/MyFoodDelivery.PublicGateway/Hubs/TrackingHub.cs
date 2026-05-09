using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MyFoodDelivery.PublicGateway.Hubs.Dtos;

namespace MyFoodDelivery.PublicGateway.Hubs;

/// <summary>
/// SignalR hub for real-time order and delivery tracking.
/// Customers subscribe to their orders, riders broadcast location updates.
/// </summary>
[Authorize]
public class TrackingHub : Hub
{
    private readonly ILogger<TrackingHub> _logger;

    public TrackingHub(ILogger<TrackingHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("Client connected: {ConnectionId}, User: {UserId}", 
            Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Customer subscribes to track a specific order.
    /// </summary>
    public async Task SubscribeToOrder(Guid orderId)
    {
        var groupName = GetOrderGroupName(orderId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "User {UserId} subscribed to order {OrderId}",
            Context.UserIdentifier, orderId);

        await Clients.Caller.SendAsync("SubscriptionConfirmed", new
        {
            OrderId = orderId,
            Message = "You are now tracking this order"
        });
    }

    /// <summary>
    /// Customer unsubscribes from order tracking.
    /// </summary>
    public async Task UnsubscribeFromOrder(Guid orderId)
    {
        var groupName = GetOrderGroupName(orderId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "User {UserId} unsubscribed from order {OrderId}",
            Context.UserIdentifier, orderId);
    }

    /// <summary>
    /// Rider updates their location (called frequently during delivery).
    /// </summary>
    [Authorize(Roles = "Rider")]
    public async Task UpdateRiderLocation(RiderLocationUpdate update)
    {
        var riderId = GetRiderIdFromClaims();
        
        _logger.LogDebug(
            "Rider {RiderId} location update: ({Lat}, {Lng})",
            riderId, update.Latitude, update.Longitude);

        // If rider has an active order, broadcast to that order's subscribers
        if (update.CurrentOrderId.HasValue)
        {
            var groupName = GetOrderGroupName(update.CurrentOrderId.Value);
            
            await Clients.Group(groupName).SendAsync("RiderLocationUpdated", new
            {
                RiderId = riderId,
                OrderId = update.CurrentOrderId.Value,
                Latitude = update.Latitude,
                Longitude = update.Longitude,
                Heading = update.Heading,
                Speed = update.Speed,
                Timestamp = DateTime.UtcNow,
                EstimatedArrivalMinutes = update.EstimatedArrivalMinutes
            });
        }
    }

    /// <summary>
    /// Restaurant marks order as ready for pickup.
    /// </summary>
    [Authorize(Roles = "Restaurant")]
    public async Task NotifyOrderReady(Guid orderId)
    {
        var groupName = GetOrderGroupName(orderId);
        
        await Clients.Group(groupName).SendAsync("OrderStatusChanged", new
        {
            OrderId = orderId,
            Status = "ReadyForPickup",
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Order {OrderId} marked as ready", orderId);
    }

    /// <summary>
    /// Broadcasts order status change to all subscribers.
    /// Called from backend services via hub context.
    /// </summary>
    public static async Task BroadcastOrderStatusAsync(
        IHubContext<TrackingHub> hubContext,
        Guid orderId,
        string status,
        object? additionalData = null)
    {
        var groupName = GetOrderGroupName(orderId);
        
        await hubContext.Clients.Group(groupName).SendAsync("OrderStatusChanged", new
        {
            OrderId = orderId,
            Status = status,
            Timestamp = DateTime.UtcNow,
            Data = additionalData
        });
    }

    /// <summary>
    /// Broadcasts rider assignment to order subscribers.
    /// </summary>
    public static async Task BroadcastRiderAssignedAsync(
        IHubContext<TrackingHub> hubContext,
        Guid orderId,
        Guid riderId,
        string riderName,
        string riderPhone,
        double riderLatitude,
        double riderLongitude,
        int estimatedMinutes)
    {
        var groupName = GetOrderGroupName(orderId);
        
        await hubContext.Clients.Group(groupName).SendAsync("RiderAssigned", new
        {
            OrderId = orderId,
            RiderId = riderId,
            RiderName = riderName,
            RiderPhone = riderPhone,
            RiderLatitude = riderLatitude,
            RiderLongitude = riderLongitude,
            EstimatedArrivalMinutes = estimatedMinutes,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Broadcasts ETA update.
    /// </summary>
    public static async Task BroadcastEtaUpdateAsync(
        IHubContext<TrackingHub> hubContext,
        Guid orderId,
        DateTime estimatedDeliveryTime,
        int estimatedMinutes)
    {
        var groupName = GetOrderGroupName(orderId);
        
        await hubContext.Clients.Group(groupName).SendAsync("EtaUpdated", new
        {
            OrderId = orderId,
            EstimatedDeliveryTime = estimatedDeliveryTime,
            EstimatedMinutes = estimatedMinutes,
            Timestamp = DateTime.UtcNow
        });
    }

    private static string GetOrderGroupName(Guid orderId) => $"order:{orderId}";

    private Guid GetRiderIdFromClaims()
    {
        var userIdClaim = Context.User?.FindFirst("sub") ?? Context.User?.FindFirst("id");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new HubException("Rider ID not found in claims");
        return userId;
    }
}
