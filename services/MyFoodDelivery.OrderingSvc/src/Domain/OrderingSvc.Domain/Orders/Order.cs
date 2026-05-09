using System;
using System.Collections.Generic;
using System.Linq;
using OrderingSvc.Domain.Orders.Events;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using OrderCancellationReason = MyFoodDelivery.Shared.Events.OrderCancellationReason;

namespace OrderingSvc.Domain.Orders;

/// <summary>
/// Order aggregate root - manages the order lifecycle with state machine pattern.
/// References CustomerId, RestaurantId (no direct navigation).
/// </summary>
public class Order : FullAuditedAggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = new();

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string? RestaurantName { get; private set; }
    public Guid? RiderId { get; private set; }
    public string? RiderName { get; private set; }
    public string? RiderPhone { get; private set; }
    public double? RiderLatitude { get; private set; }
    public double? RiderLongitude { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; } = default!;
    public string? SpecialInstructions { get; private set; }
    
    // Pricing
    public decimal SubTotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal ServiceFee { get; private set; }
    public decimal Discount { get; private set; }
    public int LoyaltyPointsUsed { get; private set; }
    public decimal TotalAmount { get; private set; }
    
    // Tracking
    public DateTime? PaymentConfirmedAt { get; private set; }
    public DateTime? PreparationStartedAt { get; private set; }
    public DateTime? ReadyForPickupAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    
    // Estimated times
    public int EstimatedPreparationMinutes { get; private set; }
    public int EstimatedDeliveryMinutes { get; private set; }
    public DateTime? EstimatedDeliveryTime { get; private set; }

    // Payment
    public int PaymentMethod { get; private set; }   // maps to PaymentMethod enum (0=Cash,1=Card,2=Wallet)
    public Guid? PaymentMethodId { get; private set; } // saved card / wallet id

    private Order() { } // EF Core

    /// <summary>
    /// Creates a new order in Pending status.
    /// </summary>
    public Order(
        Guid id,
        Guid customerId,
        Guid restaurantId,
        string orderNumber,
        DeliveryAddress deliveryAddress,
        string? specialInstructions,
        decimal deliveryFee,
        decimal serviceFee,
        int estimatedPreparationMinutes,
        int paymentMethod = 0,
        Guid? paymentMethodId = null,
        string? restaurantName = null)
        : base(id)
    {
        CustomerId = customerId;
        RestaurantId = restaurantId;
        RestaurantName = restaurantName;
        OrderNumber = orderNumber ?? throw new ArgumentNullException(nameof(orderNumber));
        DeliveryAddress = deliveryAddress ?? throw new ArgumentNullException(nameof(deliveryAddress));
        SpecialInstructions = specialInstructions;
        DeliveryFee = deliveryFee;
        ServiceFee = serviceFee;
        EstimatedPreparationMinutes = estimatedPreparationMinutes;
        PaymentMethod = paymentMethod;
        PaymentMethodId = paymentMethodId;
        Status = OrderStatus.Pending;
        
        AddLocalEvent(new OrderCreatedDomainEvent(Id, customerId, restaurantId));
    }

    #region Item Management

    public void AddItem(
        Guid menuItemId,
        string name,
        int quantity,
        decimal unitPrice,
        string? specialInstructions = null)
    {
        EnsureCanModify();

        if (quantity <= 0)
            throw new BusinessException("Order:InvalidQuantity");

        var existingItem = _items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(new OrderItem(Guid.NewGuid(), menuItemId, name, quantity, unitPrice, specialInstructions));
        }

        RecalculateTotals();
    }

    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        EnsureCanModify();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new BusinessException("Order:ItemNotFound");

        if (newQuantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(newQuantity);
        }

        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureCanModify();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new BusinessException("Order:ItemNotFound");

        _items.Remove(item);
        RecalculateTotals();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        EnsureCanModify();

        if (discountAmount < 0)
            throw new BusinessException("Order:InvalidDiscount");

        Discount = discountAmount;
        RecalculateTotals();
    }

    public void UseLoyaltyPoints(int points, decimal pointsValue)
    {
        EnsureCanModify();

        if (points < 0)
            throw new BusinessException("Order:InvalidPoints");

        LoyaltyPointsUsed = points;
        Discount += pointsValue;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.TotalPrice);
        TotalAmount = Math.Max(0, SubTotal + DeliveryFee + ServiceFee - Discount);
    }

    #endregion

    #region State Machine Transitions

    public void ConfirmPayment()
    {
        EnsureStatus(OrderStatus.Pending);
        
        if (!_items.Any())
            throw new BusinessException("Order:NoItems");

        Status = OrderStatus.PaymentConfirmed;
        PaymentConfirmedAt = DateTime.UtcNow;
        
        AddLocalEvent(new OrderPaymentConfirmedDomainEvent(Id, CustomerId, RestaurantId, TotalAmount));
    }

    public void StartPreparation()
    {
        EnsureStatus(OrderStatus.PaymentConfirmed);
        
        Status = OrderStatus.Preparing;
        PreparationStartedAt = DateTime.UtcNow;
        EstimatedDeliveryTime = DateTime.UtcNow
            .AddMinutes(EstimatedPreparationMinutes)
            .AddMinutes(EstimatedDeliveryMinutes);
        
        AddLocalEvent(new OrderPreparationStartedDomainEvent(Id, RestaurantId));
    }

    public void MarkReadyForPickup()
    {
        EnsureStatus(OrderStatus.Preparing);
        
        Status = OrderStatus.ReadyForPickup;
        ReadyForPickupAt = DateTime.UtcNow;
        
        AddLocalEvent(new OrderReadyForPickupDomainEvent(Id, RestaurantId));
    }

    public void AssignRider(Guid riderId, string riderName, string? riderPhone, int estimatedDeliveryMinutes)
    {
        if (Status != OrderStatus.ReadyForPickup && Status != OrderStatus.PaymentConfirmed)
            throw new BusinessException("Order:InvalidStatusForRiderAssignment");

        RiderId = riderId;
        RiderName = riderName;
        RiderPhone = riderPhone;
        EstimatedDeliveryMinutes = estimatedDeliveryMinutes;
        EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(estimatedDeliveryMinutes);
        
        if (Status == OrderStatus.ReadyForPickup)
        {
            Status = OrderStatus.AwaitingPickup;
        }
        
        AddLocalEvent(new RiderAssignedDomainEvent(Id, riderId, riderName));
    }

    /// <summary>
    /// Updates the rider's last known location (called when location update events are received).
    /// </summary>
    public void UpdateRiderLocation(double latitude, double longitude)
    {
        if (!RiderId.HasValue)
            throw new BusinessException("Order:NoRiderAssigned");

        RiderLatitude = latitude;
        RiderLongitude = longitude;
    }

    public void MarkPickedUp()
    {
        if (RiderId == null)
            throw new BusinessException("Order:NoRiderAssigned");
        
        EnsureStatus(OrderStatus.AwaitingPickup, OrderStatus.ReadyForPickup);
        
        Status = OrderStatus.InTransit;
        PickedUpAt = DateTime.UtcNow;
        
        AddLocalEvent(new OrderPickedUpDomainEvent(Id, RiderId.Value));
    }

    public void MarkDelivered()
    {
        EnsureStatus(OrderStatus.InTransit);
        
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        
        var loyaltyPointsEarned = CalculateLoyaltyPoints();
        
        AddLocalEvent(new OrderDeliveredDomainEvent(Id, CustomerId, RiderId!.Value, loyaltyPointsEarned));
    }

    public void Complete()
    {
        EnsureStatus(OrderStatus.Delivered);
        
        Status = OrderStatus.Completed;
        
        AddLocalEvent(new OrderCompletedDomainEvent(Id, CustomerId, RestaurantId, RiderId!.Value, TotalAmount));
    }

    public void Cancel(string reason, OrderCancellationReason cancellationReason)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.PaymentConfirmed)
            throw new BusinessException("Order:CannotCancel");

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        
        AddLocalEvent(new OrderCancelledDomainEvent(Id, CustomerId, RiderId, previousStatus, cancellationReason, reason));
    }

    #endregion

    #region Helpers

    private void EnsureCanModify()
    {
        if (Status != OrderStatus.Pending)
            throw new BusinessException("Order:CannotModify");
    }

    private void EnsureStatus(params OrderStatus[] allowedStatuses)
    {
        if (!allowedStatuses.Contains(Status))
            throw new BusinessException("Order:InvalidStatusTransition")
                .WithData("CurrentStatus", Status)
                .WithData("AllowedStatuses", string.Join(", ", allowedStatuses));
    }

    private int CalculateLoyaltyPoints()
    {
        // 1 point per dollar spent (configurable)
        return (int)Math.Floor(TotalAmount);
    }

    public void UpdateEstimatedDeliveryTime(DateTime newEta)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled || Status == OrderStatus.Delivered)
            return;

        EstimatedDeliveryTime = newEta;
        AddLocalEvent(new DeliveryEtaUpdatedDomainEvent(Id, newEta));
    }

    #endregion
}

/// <summary>
/// Order status enum representing the state machine states.
/// </summary>
public enum OrderStatus
{
    Pending,
    PaymentConfirmed,
    Preparing,
    ReadyForPickup,
    AwaitingPickup,
    InTransit,
    Delivered,
    Completed,
    Cancelled
}


