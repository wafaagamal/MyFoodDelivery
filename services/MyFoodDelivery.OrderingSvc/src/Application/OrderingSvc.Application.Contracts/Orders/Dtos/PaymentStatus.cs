namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}

