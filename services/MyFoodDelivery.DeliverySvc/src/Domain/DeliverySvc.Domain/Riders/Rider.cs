using Volo.Abp.Domain.Entities.Auditing;

namespace DeliverySvc.Domain.Riders;

/// <summary>
/// Rider aggregate root.
/// Id == IdentityUser.Id from AuthSvc (shared GUID by convention, no FK).
/// Identity data (name, email, phone) lives in AuthSvc — get from JWT claims.
/// This entity owns only rider-domain data: vehicle, status, location, earnings, rating.
/// </summary>
public class Rider : FullAuditedAggregateRoot<Guid>
{
    public string? VehicleType { get; private set; }
    public string? VehiclePlate { get; private set; }
    public RiderStatus Status { get; private set; }
    public bool IsOnline { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public DateTime? LastLocationUpdate { get; private set; }
    public DateTime? LastOnlineAt { get; private set; }
    public DateTime? LastOfflineAt { get; private set; }
    public int TotalDeliveries { get; private set; }
    public decimal TotalEarnings { get; private set; }
    public double AverageRating { get; private set; }
    public int RatingCount { get; private set; }
    public bool IsActive { get; private set; }

    protected Rider() { } // EF Core

    /// <summary>
    /// Creates a new Rider. Id = IdentityUser.Id.
    /// </summary>
    public Rider(Guid id) : base(id)
    {
        Status = RiderStatus.Offline;
        IsOnline = false;
        TotalDeliveries = 0;
        TotalEarnings = 0;
        AverageRating = 0;
        RatingCount = 0;
        IsActive = true;
    }

    public void UpdateVehicle(string? vehicleType, string? vehiclePlate)
    {
        VehicleType = vehicleType;
        VehiclePlate = vehiclePlate;
    }

    public void UpdateStatus(RiderStatus status)
    {
        Status = status;
        IsOnline = status == RiderStatus.Available || status == RiderStatus.OnDelivery;
        if (IsOnline) LastOnlineAt = DateTime.UtcNow;
        else LastOfflineAt = DateTime.UtcNow;
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        LastLocationUpdate = DateTime.UtcNow;
    }

    public void RecordDelivery(decimal earnings, double rating)
    {
        TotalDeliveries++;
        TotalEarnings += earnings;
        // Incremental average
        AverageRating = ((AverageRating * RatingCount) + rating) / (RatingCount + 1);
        RatingCount++;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
