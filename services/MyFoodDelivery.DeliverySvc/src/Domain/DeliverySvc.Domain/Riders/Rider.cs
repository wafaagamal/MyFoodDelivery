using Volo.Abp.Domain.Entities.Auditing;

namespace DeliverySvc.Domain.Riders;

public class Rider : FullAuditedAggregateRoot<Guid>
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? VehicleType { get; private set; }
    public string? VehiclePlate { get; private set; }
    public RiderStatus Status { get; private set; }
    public double? LastLatitude { get; private set; }
    public double? LastLongitude { get; private set; }
    public DateTime? LastLocationUpdate { get; private set; }
    public double Rating { get; private set; }
    public int TotalDeliveries { get; private set; }
    public bool IsActive { get; private set; }

    protected Rider() { }

    public Rider(
        Guid id,
        string firstName,
        string lastName,
        string phoneNumber,
        string? email = null,
        string? vehicleType = null,
        string? vehiclePlate = null) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Email = email;
        VehicleType = vehicleType;
        VehiclePlate = vehiclePlate;
        Status = RiderStatus.Offline;
        Rating = 5.0;
        TotalDeliveries = 0;
        IsActive = true;
    }

    public void UpdateStatus(RiderStatus status)
    {
        Status = status;
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        LastLatitude = latitude;
        LastLongitude = longitude;
        LastLocationUpdate = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName, string? email, string? vehicleType, string? vehiclePlate)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        VehicleType = vehicleType;
        VehiclePlate = vehiclePlate;
    }

    public void RecordDelivery(double rating)
    {
        TotalDeliveries++;
        Rating = ((Rating * (TotalDeliveries - 1)) + rating) / TotalDeliveries;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public string FullName => $"{FirstName} {LastName}";
}
