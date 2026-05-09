using DeliverySvc.Domain.Riders;
using DeliverySvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeliverySvc.HttpApi.Host;

/// <summary>
/// Seeds sample rider domain data (vehicle, status, location).
/// Identity data (name, email, phone) lives in AuthSvc — not seeded here.
/// </summary>
public class RiderDataSeeder
{
    private readonly DeliverySvcDbContext _dbContext;
    private readonly ILogger<RiderDataSeeder> _logger;

    public RiderDataSeeder(DeliverySvcDbContext dbContext, ILogger<RiderDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _dbContext.Riders.AnyAsync())
        {
            _logger.LogInformation("Rider data already exists. Skipping seed.");
            return;
        }

        _logger.LogInformation("Seeding rider data...");
        var riders = CreateSampleRiders();
        await _dbContext.Riders.AddRangeAsync(riders);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} riders.", riders.Count);
    }

    private List<Rider> CreateSampleRiders()
    {
        return new List<Rider>
        {
            CreateRider(Guid.Parse("dddd1111-1111-1111-1111-111111111111"), "Motorcycle", "NYC-M101", 40.7580, -73.9855),
            CreateRider(Guid.Parse("dddd2222-2222-2222-2222-222222222222"), "Bicycle", null, 40.7247, -73.9973),
            CreateRider(Guid.Parse("dddd3333-3333-3333-3333-333333333333"), "Car", "NYC-C102", 40.7629, -73.9712),
            CreateRider(Guid.Parse("dddd4444-4444-4444-4444-444444444444"), "Motorcycle", "NYC-M103", 40.7565, -73.9903),
            CreateRider(Guid.Parse("dddd5555-5555-5555-5555-555555555555"), "Electric Scooter", "NYC-E104", 40.7587, -73.9687),
            CreateRider(Guid.Parse("dddd6666-6666-6666-6666-666666666666"), "Bicycle", null, 40.7735, -73.9635),
            CreateRider(Guid.Parse("dddd7777-7777-7777-7777-777777777777"), "Car", "NYC-C105", 40.7614, -73.9705),
            CreateRider(Guid.Parse("dddd8888-8888-8888-8888-888888888888"), "Motorcycle", "NYC-M106", 40.7442, -73.9958),
            CreateRider(Guid.Parse("dddd9999-9999-9999-9999-999999999999"), "Electric Scooter", "NYC-E107", 40.7387, -73.9819),
            CreateRider(Guid.Parse("ddddaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Bicycle", null, 40.7540, -73.9725)
        };
    }

    private Rider CreateRider(Guid id, string? vehicleType, string? vehiclePlate, double latitude, double longitude)
    {
        var rider = new Rider(id);
        rider.UpdateVehicle(vehicleType, vehiclePlate);
        rider.UpdateLocation(latitude, longitude);
        rider.UpdateStatus(RiderStatus.Available);
        return rider;
    }
}
