using System;
using DeliverySvc.Domain.Riders;

namespace DeliverySvc.Application.Contracts.Riders.Dtos;

/// <summary>
/// Rider domain data only. Name/email/phone come from JWT claims in AuthSvc.
/// </summary>
public class RiderDto
{
    public Guid Id { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public RiderStatus Status { get; set; }
    public bool IsOnline { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public int TotalDeliveries { get; set; }
    public decimal TotalEarnings { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public bool IsActive { get; set; }
}
