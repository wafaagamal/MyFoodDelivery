using System;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class RestaurantLocationDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

