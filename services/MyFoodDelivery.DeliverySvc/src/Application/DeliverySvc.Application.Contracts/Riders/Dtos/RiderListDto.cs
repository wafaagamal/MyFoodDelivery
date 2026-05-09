using System;
using DeliverySvc.Domain.Riders;

namespace DeliverySvc.Application.Contracts.Riders.Dtos;

public class RiderListDto
{
    public Guid Id { get; set; }
    public RiderStatus Status { get; set; }
    public bool IsOnline { get; set; }
    public double AverageRating { get; set; }
    public int TotalDeliveries { get; set; }
    public bool IsActive { get; set; }
}
