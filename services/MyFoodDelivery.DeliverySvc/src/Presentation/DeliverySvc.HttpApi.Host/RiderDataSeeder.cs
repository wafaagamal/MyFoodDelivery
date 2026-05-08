using DeliverySvc.Domain.Riders;
using DeliverySvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeliverySvc.HttpApi.Host;

/// <summary>
/// Seeds sample rider (driver) data into the database for development/testing.
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
        // Check if data already exists
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
            CreateRider(
                Guid.Parse("dddd1111-1111-1111-1111-111111111111"),
                "Marcus", "Rodriguez",
                "+1-555-0201", "marcus.r@delivery.com",
                "Motorcycle", "NYC-M101",
                40.7580, -73.9855),

            CreateRider(
                Guid.Parse("dddd2222-2222-2222-2222-222222222222"),
                "Kevin", "Thompson",
                "+1-555-0202", "kevin.t@delivery.com",
                "Bicycle", null,
                40.7247, -73.9973),

            CreateRider(
                Guid.Parse("dddd3333-3333-3333-3333-333333333333"),
                "James", "Wilson",
                "+1-555-0203", "james.w@delivery.com",
                "Car", "NYC-C102",
                40.7629, -73.9712),

            CreateRider(
                Guid.Parse("dddd4444-4444-4444-4444-444444444444"),
                "Anthony", "Lee",
                "+1-555-0204", "anthony.l@delivery.com",
                "Motorcycle", "NYC-M103",
                40.7565, -73.9903),

            CreateRider(
                Guid.Parse("dddd5555-5555-5555-5555-555555555555"),
                "Roberto", "Santos",
                "+1-555-0205", "roberto.s@delivery.com",
                "Electric Scooter", "NYC-E104",
                40.7587, -73.9687),

            CreateRider(
                Guid.Parse("dddd6666-6666-6666-6666-666666666666"),
                "David", "Kim",
                "+1-555-0206", "david.k@delivery.com",
                "Bicycle", null,
                40.7735, -73.9635),

            CreateRider(
                Guid.Parse("dddd7777-7777-7777-7777-777777777777"),
                "Michael", "Chen",
                "+1-555-0207", "michael.c@delivery.com",
                "Car", "NYC-C105",
                40.7614, -73.9705),

            CreateRider(
                Guid.Parse("dddd8888-8888-8888-8888-888888888888"),
                "Carlos", "Gonzalez",
                "+1-555-0208", "carlos.g@delivery.com",
                "Motorcycle", "NYC-M106",
                40.7442, -73.9958),

            CreateRider(
                Guid.Parse("dddd9999-9999-9999-9999-999999999999"),
                "Brian", "Patel",
                "+1-555-0209", "brian.p@delivery.com",
                "Electric Scooter", "NYC-E107",
                40.7387, -73.9819),

            CreateRider(
                Guid.Parse("ddddaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Jose", "Martinez",
                "+1-555-0210", "jose.m@delivery.com",
                "Bicycle", null,
                40.7540, -73.9725)
        };
    }

    private Rider CreateRider(
        Guid id,
        string firstName, string lastName,
        string phone, string email,
        string vehicleType, string? vehiclePlate,
        double latitude, double longitude)
    {
        var rider = new Rider(
            id,
            firstName,
            lastName,
            phone,
            email,
            vehicleType,
            vehiclePlate);

        // Set initial location and mark as available
        rider.UpdateLocation(latitude, longitude);
        rider.UpdateStatus(RiderStatus.Available);

        return rider;
    }
}
