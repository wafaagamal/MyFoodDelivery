using System;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class RiderTrackingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public int? EtaMinutes { get; set; }
}

