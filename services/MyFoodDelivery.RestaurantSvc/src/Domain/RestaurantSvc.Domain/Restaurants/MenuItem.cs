using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace RestaurantSvc.Domain.Restaurants;

/// <summary>
/// Entity representing a menu item in a restaurant.
/// </summary>
public class MenuItem : Entity<Guid>
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal Price { get; private set; }
    public decimal? DiscountedPrice { get; private set; }
    public string? ImageUrl { get; private set; }
    public int PreparationTimeMinutes { get; private set; }
    public bool IsVegetarian { get; private set; }
    public bool IsVegan { get; private set; }
    public bool IsGlutenFree { get; private set; }
    public bool IsSpicy { get; private set; }
    public List<string> Allergens { get; private set; } = new();
    public int? StockQuantity { get; private set; }
    public bool IsAvailable { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsFeatured { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private MenuItem() { } // MongoDB

    internal MenuItem(
        Guid id,
        Guid categoryId,
        string name,
        string description,
        decimal price,
        string? imageUrl,
        int preparationTimeMinutes,
        bool isVegetarian,
        bool isVegan,
        bool isGlutenFree,
        bool isSpicy,
        List<string> allergens)
        : base(id)
    {
        CategoryId = categoryId;
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        ImageUrl = imageUrl;
        PreparationTimeMinutes = preparationTimeMinutes;
        IsVegetarian = isVegetarian;
        IsVegan = isVegan;
        IsGlutenFree = isGlutenFree;
        IsSpicy = isSpicy;
        Allergens = allergens;
        IsAvailable = true;
        IsActive = true;
        IsFeatured = false;
        DisplayOrder = 0;
        CreatedAt = DateTime.UtcNow;
    }

    internal void Update(
        string name,
        string description,
        decimal price,
        string? imageUrl,
        int preparationTimeMinutes,
        bool isVegetarian,
        bool isVegan,
        bool isGlutenFree,
        bool isSpicy,
        List<string> allergens)
    {
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        ImageUrl = imageUrl;
        PreparationTimeMinutes = preparationTimeMinutes;
        IsVegetarian = isVegetarian;
        IsVegan = isVegan;
        IsGlutenFree = isGlutenFree;
        IsSpicy = isSpicy;
        Allergens = allergens;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new BusinessException("Restaurant:InvalidStockQuantity");
        
        StockQuantity = quantity;
        IsAvailable = quantity > 0;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void DeductStock(int quantity)
    {
        if (!StockQuantity.HasValue) return;
        
        StockQuantity = Math.Max(0, StockQuantity.Value - quantity);
        if (StockQuantity == 0)
            IsAvailable = false;
    }

    internal void Deactivate()
    {
        IsActive = false;
        IsAvailable = false;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void SetFeatured(bool featured)
    {
        IsFeatured = featured;
    }

    internal void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }

    public void ApplyDiscount(decimal discountedPrice)
    {
        if (discountedPrice >= Price)
            throw new BusinessException("Restaurant:InvalidDiscount");
        
        DiscountedPrice = discountedPrice;
    }

    public void RemoveDiscount()
    {
        DiscountedPrice = null;
    }

    public decimal GetCurrentPrice() => DiscountedPrice ?? Price;

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Restaurant:MenuItemNameRequired");
        if (name.Length > 200)
            throw new BusinessException("Restaurant:MenuItemNameTooLong");
        Name = name.Trim();
    }

    private void SetDescription(string description)
    {
        Description = description?.Trim() ?? string.Empty;
    }

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new BusinessException("Restaurant:InvalidPrice");
        Price = price;
    }
}

/// <summary>
/// Entity representing a menu category.
/// </summary>
public class MenuCategory : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    private MenuCategory() { } // MongoDB

    internal MenuCategory(Guid id, string name, string? description, int displayOrder)
        : base(id)
    {
        SetName(name);
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = true;
    }

    internal void Update(string name, string? description, int displayOrder)
    {
        SetName(name);
        Description = description;
        DisplayOrder = displayOrder;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Restaurant:CategoryNameRequired");
        Name = name.Trim();
    }
}

/// <summary>
/// Entity representing restaurant opening hours for a day.
/// </summary>
public class OpeningHours
{
    public DayOfWeek Day { get; private set; }
    public TimeSpan OpenTime { get; private set; }
    public TimeSpan CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    private OpeningHours() { } // MongoDB

    internal OpeningHours(DayOfWeek day, TimeSpan openTime, TimeSpan closeTime, bool isClosed)
    {
        Day = day;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    internal void Update(TimeSpan openTime, TimeSpan closeTime, bool isClosed)
    {
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }
}
