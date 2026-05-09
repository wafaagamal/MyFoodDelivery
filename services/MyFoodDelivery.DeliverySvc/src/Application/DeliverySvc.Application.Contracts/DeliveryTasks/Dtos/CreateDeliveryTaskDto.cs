using System;

namespace DeliverySvc.Application.Contracts.DeliveryTasks.Dtos;

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
