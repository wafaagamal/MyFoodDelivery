using DeliverySvc.Domain.Riders;

namespace DeliverySvc.Application.Contracts.Riders;

public class RiderDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
    public RiderStatus Status { get; set; }
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public double Rating { get; set; }
    public int TotalDeliveries { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRiderDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
}

public class UpdateRiderDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public string? VehicleType { get; set; }
    public string? VehiclePlate { get; set; }
}

public class UpdateRiderLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class RiderListDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public RiderStatus Status { get; set; }
    public double Rating { get; set; }
    public int TotalDeliveries { get; set; }
    public bool IsActive { get; set; }
}
