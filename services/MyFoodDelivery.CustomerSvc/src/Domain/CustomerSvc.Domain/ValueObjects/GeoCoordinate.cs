using System;
using Volo.Abp.Domain.Values;

namespace CustomerSvc.Domain.ValueObjects;

/// <summary>
/// Value object representing a geographic coordinate (latitude/longitude).
/// </summary>
public class GeoCoordinate : ValueObject
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    private GeoCoordinate() { } // EF Core

    public GeoCoordinate(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Calculates the distance to another coordinate in kilometers using the Haversine formula.
    /// </summary>
    public double DistanceTo(GeoCoordinate other)
    {
        const double R = 6371; // Earth's radius in kilometers

        var lat1 = ToRadians(Latitude);
        var lat2 = ToRadians(other.Latitude);
        var deltaLat = ToRadians(other.Latitude - Latitude);
        var deltaLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";
}
