using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantSvc.Domain.Restaurants.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace RestaurantSvc.Domain.Restaurants;

/// <summary>
/// Restaurant aggregate root - manages restaurant info, menu, and operating hours.
/// </summary>
public class Restaurant : FullAuditedAggregateRoot<Guid>
{
    private List<MenuItem> _menuItems = new();
    private List<MenuCategory> _categories = new();
    private List<OpeningHours> _openingHours = new();

    public IReadOnlyCollection<MenuItem> MenuItems => _menuItems.AsReadOnly();
    public IReadOnlyCollection<MenuCategory> Categories => _categories.AsReadOnly();
    public IReadOnlyCollection<OpeningHours> OpeningHours => _openingHours.AsReadOnly();

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string CuisineType { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public RestaurantAddress Address { get; private set; } = default!;
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public decimal MinimumOrderAmount { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public int EstimatedDeliveryMinutes { get; private set; }
    public decimal AverageRating { get; private set; }
    public int TotalRatings { get; private set; }
    public int TotalOrders { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsOpen { get; private set; }
    public bool AcceptingOrders { get; private set; }

    private Restaurant() { } // MongoDB

    public Restaurant(
        Guid id,
        Guid ownerId,
        string name,
        string description,
        string cuisineType,
        string phoneNumber,
        string email,
        RestaurantAddress address,
        decimal minimumOrderAmount,
        decimal deliveryFee,
        int estimatedDeliveryMinutes)
        : base(id)
    {
        OwnerId = ownerId;
        SetName(name);
        SetDescription(description);
        CuisineType = cuisineType ?? throw new ArgumentNullException(nameof(cuisineType));
        SetPhoneNumber(phoneNumber);
        SetEmail(email);
        Address = address ?? throw new ArgumentNullException(nameof(address));
        MinimumOrderAmount = minimumOrderAmount;
        DeliveryFee = deliveryFee;
        EstimatedDeliveryMinutes = estimatedDeliveryMinutes;
        IsActive = false; // Needs approval
        IsOpen = false;
        AcceptingOrders = false;
        AverageRating = 0;
        TotalRatings = 0;
        TotalOrders = 0;

        AddLocalEvent(new RestaurantCreatedDomainEvent(Id, ownerId, name));
    }

    #region Restaurant Info

    public void UpdateInfo(
        string name,
        string description,
        string cuisineType,
        string phoneNumber,
        string email,
        decimal minimumOrderAmount,
        decimal deliveryFee,
        int estimatedDeliveryMinutes)
    {
        SetName(name);
        SetDescription(description);
        CuisineType = cuisineType;
        SetPhoneNumber(phoneNumber);
        SetEmail(email);
        MinimumOrderAmount = minimumOrderAmount;
        DeliveryFee = deliveryFee;
        EstimatedDeliveryMinutes = estimatedDeliveryMinutes;

        AddLocalEvent(new RestaurantUpdatedDomainEvent(Id));
    }

    public void UpdateAddress(RestaurantAddress address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public void SetLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
    }

    public void SetBanner(string bannerUrl)
    {
        BannerUrl = bannerUrl;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Restaurant:NameRequired");
        if (name.Length > 200)
            throw new BusinessException("Restaurant:NameTooLong");
        Name = name.Trim();
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException("Restaurant:DescriptionRequired");
        Description = description.Trim();
    }

    private void SetPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new BusinessException("Restaurant:PhoneRequired");
        PhoneNumber = phone.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            throw new BusinessException("Restaurant:InvalidEmail");
        Email = email.ToLowerInvariant().Trim();
    }

    #endregion

    #region Status Management

    public void Approve()
    {
        if (IsActive)
            throw new BusinessException("Restaurant:AlreadyActive");
        
        IsActive = true;
        AddLocalEvent(new RestaurantApprovedDomainEvent(Id));
    }

    public void Suspend(string reason)
    {
        if (!IsActive)
            throw new BusinessException("Restaurant:NotActive");
        
        IsActive = false;
        IsOpen = false;
        AcceptingOrders = false;
        
        AddLocalEvent(new RestaurantSuspendedDomainEvent(Id, reason));
    }

    public void Open()
    {
        if (!IsActive)
            throw new BusinessException("Restaurant:NotActive");
        
        IsOpen = true;
        AcceptingOrders = true;
        
        AddLocalEvent(new RestaurantOpenedDomainEvent(Id));
    }

    public void Close()
    {
        IsOpen = false;
        AcceptingOrders = false;
        
        AddLocalEvent(new RestaurantClosedDomainEvent(Id));
    }

    public void PauseOrders(string reason)
    {
        AcceptingOrders = false;
        AddLocalEvent(new RestaurantPausedOrdersDomainEvent(Id, reason));
    }

    public void ResumeOrders()
    {
        if (!IsOpen)
            throw new BusinessException("Restaurant:MustBeOpen");
        
        AcceptingOrders = true;
    }

    public bool CanAcceptOrders() => IsActive && IsOpen && AcceptingOrders;

    #endregion

    #region Menu Categories

    public Guid AddCategory(string name, string? description, int displayOrder)
    {
        if (_categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new BusinessException("Restaurant:CategoryExists");

        var category = new MenuCategory(Guid.NewGuid(), name, description, displayOrder);
        _categories.Add(category);

        return category.Id;
    }

    public void UpdateCategory(Guid categoryId, string name, string? description, int displayOrder)
    {
        var category = GetCategoryOrThrow(categoryId);
        category.Update(name, description, displayOrder);
    }

    public void RemoveCategory(Guid categoryId)
    {
        var category = GetCategoryOrThrow(categoryId);
        
        // Check if category has items
        if (_menuItems.Any(m => m.CategoryId == categoryId && m.IsAvailable))
            throw new BusinessException("Restaurant:CategoryHasItems");

        _categories.Remove(category);
    }

    private MenuCategory GetCategoryOrThrow(Guid categoryId)
    {
        return _categories.FirstOrDefault(c => c.Id == categoryId)
            ?? throw new BusinessException("Restaurant:CategoryNotFound");
    }

    #endregion

    #region Menu Items

    public Guid AddMenuItem(
        Guid categoryId,
        string name,
        string description,
        decimal price,
        string? imageUrl,
        int preparationTimeMinutes,
        bool isVegetarian = false,
        bool isVegan = false,
        bool isGlutenFree = false,
        bool isSpicy = false,
        List<string>? allergens = null)
    {
        var category = GetCategoryOrThrow(categoryId);

        var menuItem = new MenuItem(
            Guid.NewGuid(),
            categoryId,
            name,
            description,
            price,
            imageUrl,
            preparationTimeMinutes,
            isVegetarian,
            isVegan,
            isGlutenFree,
            isSpicy,
            allergens ?? new List<string>());

        _menuItems.Add(menuItem);

        AddLocalEvent(new MenuItemAddedDomainEvent(Id, menuItem.Id, name, price));

        return menuItem.Id;
    }

    public void UpdateMenuItem(
        Guid menuItemId,
        string name,
        string description,
        decimal price,
        string? imageUrl,
        int preparationTimeMinutes,
        bool isVegetarian,
        bool isVegan,
        bool isGlutenFree,
        bool isSpicy,
        List<string>? allergens)
    {
        var menuItem = GetMenuItemOrThrow(menuItemId);
        
        menuItem.Update(
            name, description, price, imageUrl,
            preparationTimeMinutes, isVegetarian, isVegan,
            isGlutenFree, isSpicy, allergens ?? new List<string>());

        AddLocalEvent(new MenuItemUpdatedDomainEvent(Id, menuItemId));
    }

    public void SetMenuItemAvailability(Guid menuItemId, bool isAvailable)
    {
        var menuItem = GetMenuItemOrThrow(menuItemId);
        menuItem.SetAvailability(isAvailable);

        if (!isAvailable)
        {
            AddLocalEvent(new MenuItemUnavailableDomainEvent(Id, menuItemId, menuItem.Name));
        }
    }

    public void UpdateMenuItemStock(Guid menuItemId, int quantity)
    {
        var menuItem = GetMenuItemOrThrow(menuItemId);
        menuItem.UpdateStock(quantity);
    }

    public void RemoveMenuItem(Guid menuItemId)
    {
        var menuItem = GetMenuItemOrThrow(menuItemId);
        menuItem.Deactivate();
    }

    public MenuItem GetMenuItemOrThrow(Guid menuItemId)
    {
        return _menuItems.FirstOrDefault(m => m.Id == menuItemId && m.IsActive)
            ?? throw new BusinessException("Restaurant:MenuItemNotFound");
    }

    public List<MenuItem> GetAvailableMenuItems()
    {
        return _menuItems.Where(m => m.IsActive && m.IsAvailable).ToList();
    }

    public bool ValidateOrderItems(List<(Guid MenuItemId, int Quantity)> items, out string? error)
    {
        error = null;

        foreach (var (menuItemId, quantity) in items)
        {
            var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
            
            if (menuItem == null || !menuItem.IsActive)
            {
                error = $"Menu item not found: {menuItemId}";
                return false;
            }

            if (!menuItem.IsAvailable)
            {
                error = $"{menuItem.Name} is currently unavailable";
                return false;
            }

            if (menuItem.StockQuantity.HasValue && menuItem.StockQuantity < quantity)
            {
                error = $"Insufficient stock for {menuItem.Name}";
                return false;
            }
        }

        return true;
    }

    public decimal CalculateOrderTotal(List<(Guid MenuItemId, int Quantity)> items)
    {
        decimal total = 0;
        foreach (var (menuItemId, quantity) in items)
        {
            var menuItem = _menuItems.First(m => m.Id == menuItemId);
            total += menuItem.Price * quantity;
        }
        return total;
    }

    #endregion

    #region Opening Hours

    public void SetOpeningHours(DayOfWeek day, TimeSpan openTime, TimeSpan closeTime, bool isClosed = false)
    {
        var existing = _openingHours.FirstOrDefault(h => h.Day == day);
        
        if (existing != null)
        {
            existing.Update(openTime, closeTime, isClosed);
        }
        else
        {
            _openingHours.Add(new OpeningHours(day, openTime, closeTime, isClosed));
        }
    }

    public bool IsCurrentlyOpen()
    {
        var now = DateTime.Now;
        var todayHours = _openingHours.FirstOrDefault(h => h.Day == now.DayOfWeek);
        
        if (todayHours == null || todayHours.IsClosed)
            return false;

        var currentTime = now.TimeOfDay;
        return currentTime >= todayHours.OpenTime && currentTime <= todayHours.CloseTime;
    }

    #endregion

    #region Ratings & Orders

    public void AddRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new BusinessException("Restaurant:InvalidRating");

        var totalPoints = AverageRating * TotalRatings + rating;
        TotalRatings++;
        AverageRating = Math.Round(totalPoints / TotalRatings, 2);
    }

    public void IncrementOrderCount()
    {
        TotalOrders++;
    }

    #endregion
}

/// <summary>
/// Value object for restaurant address with coordinates.
/// </summary>
public class RestaurantAddress
{
    public string Street { get; private set; } = default!;
    public string BuildingNumber { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string? District { get; private set; }
    public string PostalCode { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    private RestaurantAddress() { }

    public RestaurantAddress(
        string street, string buildingNumber, string city,
        string? district, string postalCode, string country,
        double latitude, double longitude)
    {
        Street = street;
        BuildingNumber = buildingNumber;
        City = city;
        District = district;
        PostalCode = postalCode;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string GetFullAddress() =>
        $"{Street} {BuildingNumber}, {City}, {PostalCode}, {Country}";
}
