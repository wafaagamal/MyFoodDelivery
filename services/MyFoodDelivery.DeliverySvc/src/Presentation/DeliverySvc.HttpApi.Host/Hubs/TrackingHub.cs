using Microsoft.AspNetCore.SignalR;

namespace DeliverySvc.HttpApi.Host;

/// <summary>
/// SignalR Hub for real-time rider location tracking.
/// </summary>
public class TrackingHub : Hub
{
    /// <summary>
    /// Subscribe to order tracking updates
    /// </summary>
    public async Task SubscribeToOrder(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    /// <summary>
    /// Unsubscribe from order tracking updates
    /// </summary>
    public async Task UnsubscribeFromOrder(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
    }

    /// <summary>
    /// Subscribe to rider location updates (for customers tracking their order)
    /// </summary>
    public async Task SubscribeToRider(string riderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"rider-{riderId}");
    }

    /// <summary>
    /// Unsubscribe from rider location updates
    /// </summary>
    public async Task UnsubscribeFromRider(string riderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"rider-{riderId}");
    }

    /// <summary>
    /// Called by riders to update their location
    /// </summary>
    public async Task UpdateLocation(double latitude, double longitude, double? heading, double? speed)
    {
        var riderId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(riderId))
        {
            await Clients.Group($"rider-{riderId}").SendAsync("LocationUpdated", new
            {
                RiderId = riderId,
                Latitude = latitude,
                Longitude = longitude,
                Heading = heading,
                Speed = speed,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
