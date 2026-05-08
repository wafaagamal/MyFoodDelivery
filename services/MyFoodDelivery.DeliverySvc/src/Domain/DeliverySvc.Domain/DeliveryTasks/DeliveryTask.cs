using Volo.Abp.Domain.Entities.Auditing;

namespace DeliverySvc.Domain.DeliveryTasks;

public class DeliveryTask : FullAuditedAggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid? RiderId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string PickupAddress { get; private set; } = default!;
    public double PickupLatitude { get; private set; }
    public double PickupLongitude { get; private set; }
    public string DeliveryAddress { get; private set; } = default!;
    public double DeliveryLatitude { get; private set; }
    public double DeliveryLongitude { get; private set; }
    public string? DeliveryInstructions { get; private set; }
    public DeliveryTaskStatus Status { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int EstimatedMinutes { get; private set; }

    protected DeliveryTask() { }

    public DeliveryTask(
        Guid id,
        Guid orderId,
        Guid restaurantId,
        string pickupAddress,
        double pickupLatitude,
        double pickupLongitude,
        string deliveryAddress,
        double deliveryLatitude,
        double deliveryLongitude,
        string? deliveryInstructions = null,
        int estimatedMinutes = 30) : base(id)
    {
        OrderId = orderId;
        RestaurantId = restaurantId;
        PickupAddress = pickupAddress;
        PickupLatitude = pickupLatitude;
        PickupLongitude = pickupLongitude;
        DeliveryAddress = deliveryAddress;
        DeliveryLatitude = deliveryLatitude;
        DeliveryLongitude = deliveryLongitude;
        DeliveryInstructions = deliveryInstructions;
        EstimatedMinutes = estimatedMinutes;
        Status = DeliveryTaskStatus.Pending;
    }

    public void AssignRider(Guid riderId)
    {
        RiderId = riderId;
        Status = DeliveryTaskStatus.Assigned;
        AssignedAt = DateTime.UtcNow;
    }

    public void MarkPickedUp()
    {
        Status = DeliveryTaskStatus.PickedUp;
        PickedUpAt = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        Status = DeliveryTaskStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = DeliveryTaskStatus.Failed;
        FailureReason = reason;
    }

    public void Cancel()
    {
        Status = DeliveryTaskStatus.Cancelled;
    }
}
