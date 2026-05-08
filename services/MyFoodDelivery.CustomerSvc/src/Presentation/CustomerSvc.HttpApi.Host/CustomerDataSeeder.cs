using CustomerSvc.Domain.Customers;
using CustomerSvc.Domain.ValueObjects;
using CustomerSvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CustomerSvc.HttpApi.Host;

/// <summary>
/// Seeds sample customer data into the database for development/testing.
/// </summary>
public class CustomerDataSeeder
{
    private readonly CustomerSvcDbContext _dbContext;
    private readonly ILogger<CustomerDataSeeder> _logger;

    public CustomerDataSeeder(CustomerSvcDbContext dbContext, ILogger<CustomerDataSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        if (await _dbContext.Customers.AnyAsync())
        {
            _logger.LogInformation("Customer data already exists. Skipping seed.");
            return;
        }

        _logger.LogInformation("Seeding customer data...");

        var customers = CreateSampleCustomers();
        await _dbContext.Customers.AddRangeAsync(customers);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} customers.", customers.Count);
    }

    private List<Customer> CreateSampleCustomers()
    {
        return new List<Customer>
        {
            CreateCustomer(
                Guid.Parse("aaaa1111-1111-1111-1111-111111111111"),
                "John", "Smith",
                "john.smith@email.com", "+1-555-0101",
                "123 Park Avenue", "Apt 4B", "New York", "NY", "10001", 40.7580, -73.9855),

            CreateCustomer(
                Guid.Parse("aaaa2222-2222-2222-2222-222222222222"),
                "Emily", "Johnson",
                "emily.johnson@email.com", "+1-555-0102",
                "456 Broadway", null, "New York", "NY", "10012", 40.7247, -73.9973),

            CreateCustomer(
                Guid.Parse("aaaa3333-3333-3333-3333-333333333333"),
                "Michael", "Williams",
                "michael.williams@email.com", "+1-555-0103",
                "789 Fifth Avenue", "Suite 100", "New York", "NY", "10022", 40.7629, -73.9712),

            CreateCustomer(
                Guid.Parse("aaaa4444-4444-4444-4444-444444444444"),
                "Sarah", "Brown",
                "sarah.brown@email.com", "+1-555-0104",
                "321 West 42nd Street", null, "New York", "NY", "10036", 40.7565, -73.9903),

            CreateCustomer(
                Guid.Parse("aaaa5555-5555-5555-5555-555555555555"),
                "David", "Garcia",
                "david.garcia@email.com", "+1-555-0105",
                "654 Lexington Avenue", "Floor 3", "New York", "NY", "10022", 40.7587, -73.9687),

            CreateCustomer(
                Guid.Parse("aaaa6666-6666-6666-6666-666666666666"),
                "Jessica", "Miller",
                "jessica.miller@email.com", "+1-555-0106",
                "987 Madison Avenue", null, "New York", "NY", "10021", 40.7735, -73.9635),

            CreateCustomer(
                Guid.Parse("aaaa7777-7777-7777-7777-777777777777"),
                "Daniel", "Davis",
                "daniel.davis@email.com", "+1-555-0107",
                "147 East 57th Street", "Apt 12C", "New York", "NY", "10022", 40.7614, -73.9705),

            CreateCustomer(
                Guid.Parse("aaaa8888-8888-8888-8888-888888888888"),
                "Ashley", "Martinez",
                "ashley.martinez@email.com", "+1-555-0108",
                "258 West 23rd Street", null, "New York", "NY", "10011", 40.7442, -73.9958),

            CreateCustomer(
                Guid.Parse("aaaa9999-9999-9999-9999-999999999999"),
                "Christopher", "Anderson",
                "chris.anderson@email.com", "+1-555-0109",
                "369 Second Avenue", "Apt 5A", "New York", "NY", "10010", 40.7387, -73.9819),

            CreateCustomer(
                Guid.Parse("aaaabbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                "Amanda", "Taylor",
                "amanda.taylor@email.com", "+1-555-0110",
                "741 Third Avenue", null, "New York", "NY", "10017", 40.7540, -73.9725)
        };
    }

    private Customer CreateCustomer(
        Guid id,
        string firstName, string lastName,
        string email, string phone,
        string street, string? unit, string city, string state, string zipCode,
        double latitude, double longitude)
    {
        var customer = new Customer(
            id,
            firstName,
            lastName,
            new Email(email),
            new PhoneNumber(phone));

        // Add a delivery address
        customer.AddDeliveryAddress(
            label: "Home",
            street: street,
            buildingNumber: "1",
            floor: null,
            apartment: unit,
            city: city,
            district: state,
            postalCode: zipCode,
            country: "USA",
            coordinates: new GeoCoordinate(latitude, longitude),
            deliveryInstructions: "Ring doorbell",
            isDefault: true);

        return customer;
    }
}
