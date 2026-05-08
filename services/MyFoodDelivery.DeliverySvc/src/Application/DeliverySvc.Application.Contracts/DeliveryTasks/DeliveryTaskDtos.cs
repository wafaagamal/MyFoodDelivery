using DeliverySvc.Domain.DeliveryTasks;

namespace DeliverySvc.Application.Contracts.DeliveryTasks;

public class DeliveryTaskDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? RiderId { get; set; }
    public string? RiderName { get; set; }
    public Guid RestaurantId { get; set; }
    public string PickupAddress { get; set; } = default!;
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }
    public string DeliveryAddress { get; set; } = default!;
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? DeliveryInstructions { get; set; }
    public DeliveryTaskStatus Status { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int EstimatedMinutes { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateDeliveryTaskDto
{
    public Guid OrderId { get; set; }
    public Guid RestaurantId { get; set; }
    public string PickupAddress { get; set; } = default!;
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }
    public string DeliveryAddress { get; set; } = default!;
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public string? DeliveryInstructions { get; set; }
    public int EstimatedMinutes { get; set; } = 30;
}

public class AssignRiderDto
{
    public Guid RiderId { get; set; }
}
