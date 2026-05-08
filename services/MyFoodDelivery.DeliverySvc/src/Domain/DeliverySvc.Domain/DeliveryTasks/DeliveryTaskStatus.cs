namespace DeliverySvc.Domain.DeliveryTasks;

public enum DeliveryTaskStatus
{
    Pending = 0,
    Assigned = 1,
    PickedUp = 2,
    Delivered = 3,
    Failed = 4,
    Cancelled = 5
}
