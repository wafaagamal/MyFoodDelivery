using MongoDB.Driver;
using RestaurantSvc.Domain.Restaurants;

namespace RestaurantSvc.HttpApi.Host;

/// <summary>
/// Seeds sample restaurant and menu data into MongoDB for development/testing.
/// </summary>
public class RestaurantDataSeeder
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<RestaurantDataSeeder> _logger;

    public RestaurantDataSeeder(IMongoDatabase database, ILogger<RestaurantDataSeeder> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var collection = _database.GetCollection<Restaurant>("restaurants");
        
        // Check if data already exists
        var existingCount = await collection.CountDocumentsAsync(_ => true);
        if (existingCount > 0)
        {
            _logger.LogInformation("Restaurant data already exists. Skipping seed.");
            return;
        }

        _logger.LogInformation("Seeding restaurant data...");

        var restaurants = CreateSampleRestaurants();
        await collection.InsertManyAsync(restaurants);

        _logger.LogInformation("Seeded {Count} restaurants with menu items.", restaurants.Count);
    }

    private List<Restaurant> CreateSampleRestaurants()
    {
        var restaurants = new List<Restaurant>();

        // Restaurant 1: Italian Restaurant
        var italianRestaurant = CreateRestaurant(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Mario's Italian Kitchen",
            "Authentic Italian cuisine with homemade pasta and wood-fired pizzas",
            "Italian",
            "123 Main Street", "New York", "NY", "10001", "USA", 40.7128, -74.0060);
        
        AddMenuItems(italianRestaurant, new[]
        {
            ("Margherita Pizza", "Classic pizza with tomato, mozzarella, and basil", 14.99m, true, false, 15),
            ("Spaghetti Carbonara", "Creamy pasta with pancetta, egg, and parmesan", 16.99m, false, false, 20),
            ("Lasagna", "Layered pasta with meat sauce and béchamel", 18.99m, false, false, 25),
            ("Tiramisu", "Classic Italian dessert with coffee and mascarpone", 8.99m, true, false, 5),
            ("Bruschetta", "Toasted bread with tomatoes, garlic, and olive oil", 9.99m, true, true, 10),
            ("Risotto ai Funghi", "Creamy mushroom risotto", 17.99m, true, false, 25),
            ("Penne Arrabbiata", "Spicy tomato pasta with garlic and chili", 13.99m, true, true, 15),
            ("Caprese Salad", "Fresh mozzarella, tomatoes, and basil", 11.99m, true, true, 8)
        });
        restaurants.Add(italianRestaurant);

        // Restaurant 2: Chinese Restaurant
        var chineseRestaurant = CreateRestaurant(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Golden Dragon",
            "Traditional Chinese dishes with authentic flavors from Szechuan and Cantonese regions",
            "Chinese",
            "456 Oak Avenue", "New York", "NY", "10002", "USA", 40.7200, -73.9950);

        AddMenuItems(chineseRestaurant, new[]
        {
            ("Kung Pao Chicken", "Spicy stir-fried chicken with peanuts and vegetables", 15.99m, false, false, 18),
            ("Sweet and Sour Pork", "Crispy pork with tangy sweet and sour sauce", 16.99m, false, false, 20),
            ("Fried Rice", "Wok-fried rice with vegetables and egg", 12.99m, true, false, 12),
            ("Spring Rolls", "Crispy vegetable spring rolls", 7.99m, true, true, 10),
            ("Mapo Tofu", "Spicy tofu in chili bean sauce", 13.99m, true, true, 15),
            ("Dim Sum Platter", "Assorted steamed dumplings", 18.99m, false, false, 25),
            ("Hot and Sour Soup", "Traditional spicy and tangy soup", 6.99m, true, false, 10),
            ("Beef Chow Mein", "Stir-fried noodles with beef and vegetables", 14.99m, false, false, 18)
        });
        restaurants.Add(chineseRestaurant);

        // Restaurant 3: Mexican Restaurant
        var mexicanRestaurant = CreateRestaurant(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Casa de Tacos",
            "Fresh Mexican street food with handmade tortillas and authentic recipes",
            "Mexican",
            "789 Elm Street", "New York", "NY", "10003", "USA", 40.7300, -73.9900);

        AddMenuItems(mexicanRestaurant, new[]
        {
            ("Tacos al Pastor", "Marinated pork tacos with pineapple and cilantro", 12.99m, false, false, 15),
            ("Chicken Burrito", "Large flour tortilla filled with chicken, rice, and beans", 14.99m, false, false, 15),
            ("Guacamole & Chips", "Fresh avocado dip with crispy tortilla chips", 9.99m, true, true, 10),
            ("Quesadilla", "Grilled cheese tortilla with your choice of filling", 11.99m, true, false, 12),
            ("Nachos Supreme", "Tortilla chips loaded with cheese, beans, and toppings", 13.99m, true, false, 15),
            ("Enchiladas", "Corn tortillas rolled with chicken and covered in sauce", 15.99m, false, false, 20),
            ("Churros", "Fried dough pastry with cinnamon sugar and chocolate sauce", 6.99m, true, false, 8),
            ("Elote", "Mexican street corn with mayo, cheese, and chili", 5.99m, true, true, 10)
        });
        restaurants.Add(mexicanRestaurant);

        // Restaurant 4: Indian Restaurant
        var indianRestaurant = CreateRestaurant(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Spice of India",
            "Rich and flavorful Indian cuisine featuring curries, tandoori, and biryanis",
            "Indian",
            "321 Maple Drive", "New York", "NY", "10004", "USA", 40.7050, -74.0100);

        AddMenuItems(indianRestaurant, new[]
        {
            ("Chicken Tikka Masala", "Grilled chicken in creamy tomato sauce", 17.99m, false, false, 25),
            ("Vegetable Biryani", "Fragrant basmati rice with mixed vegetables", 14.99m, true, true, 30),
            ("Butter Chicken", "Tender chicken in rich buttery tomato gravy", 18.99m, false, false, 25),
            ("Samosas", "Crispy pastries filled with spiced potatoes", 6.99m, true, true, 15),
            ("Naan Bread", "Traditional Indian flatbread", 3.99m, true, false, 8),
            ("Palak Paneer", "Spinach curry with cottage cheese", 15.99m, true, true, 20),
            ("Lamb Vindaloo", "Spicy lamb curry with potatoes", 19.99m, false, false, 30),
            ("Mango Lassi", "Sweet yogurt drink with mango", 4.99m, true, true, 5)
        });
        restaurants.Add(indianRestaurant);

        // Restaurant 5: Japanese Restaurant
        var japaneseRestaurant = CreateRestaurant(
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Sakura Sushi",
            "Fresh sushi, sashimi, and traditional Japanese dishes",
            "Japanese",
            "555 Cherry Lane", "New York", "NY", "10005", "USA", 40.7150, -74.0050);

        AddMenuItems(japaneseRestaurant, new[]
        {
            ("Salmon Sushi Roll", "Fresh salmon with avocado and cucumber", 14.99m, false, false, 15),
            ("Chicken Teriyaki", "Grilled chicken with teriyaki sauce and rice", 16.99m, false, false, 20),
            ("Miso Soup", "Traditional Japanese soup with tofu and seaweed", 4.99m, true, true, 8),
            ("Tempura", "Lightly battered and fried shrimp and vegetables", 13.99m, false, false, 15),
            ("Edamame", "Steamed soybean pods with sea salt", 5.99m, true, true, 8),
            ("Ramen", "Japanese noodle soup with pork and egg", 15.99m, false, false, 20),
            ("California Roll", "Crab, avocado, and cucumber roll", 12.99m, false, false, 12),
            ("Green Tea Ice Cream", "Traditional matcha flavored ice cream", 6.99m, true, true, 5)
        });
        restaurants.Add(japaneseRestaurant);

        return restaurants;
    }

    private Restaurant CreateRestaurant(
        Guid id,
        string name,
        string description,
        string cuisineType,
        string street, string city, string state, string postalCode, string country,
        double latitude, double longitude)
    {
        var address = new RestaurantAddress(
            street: street, 
            buildingNumber: "1", 
            city: city, 
            district: state, 
            postalCode: postalCode, 
            country: country, 
            latitude: latitude, 
            longitude: longitude);
        var restaurant = new Restaurant(
            id,
            ownerId: Guid.NewGuid(),
            name: name,
            description: description,
            cuisineType: cuisineType,
            phoneNumber: "555-123-4567",
            email: $"info@{name.ToLower().Replace(" ", "").Replace("'", "")}.com",
            address: address,
            minimumOrderAmount: 15.00m,
            deliveryFee: 3.99m,
            estimatedDeliveryMinutes: 30);

        // Activate the restaurant for demo
        restaurant.Approve();
        restaurant.Open();

        return restaurant;
    }

    private void AddMenuItems(Restaurant restaurant, (string Name, string Description, decimal Price, bool IsVegetarian, bool IsVegan, int PrepTime)[] items)
    {
        var categoryId = restaurant.AddCategory("Main Menu", "Our delicious offerings", 1);

        foreach (var item in items)
        {
            restaurant.AddMenuItem(
                categoryId: categoryId,
                name: item.Name,
                description: item.Description,
                price: item.Price,
                imageUrl: null,
                preparationTimeMinutes: item.PrepTime,
                isVegetarian: item.IsVegetarian,
                isVegan: item.IsVegan,
                isGlutenFree: false,
                isSpicy: item.Name.Contains("Spicy") || item.Name.Contains("Hot") || item.Name.Contains("Kung Pao") || item.Name.Contains("Vindaloo") || item.Name.Contains("Arrabbiata"),
                allergens: new List<string>());
        }
    }
}
