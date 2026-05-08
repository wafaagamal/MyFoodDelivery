using System;
using System.Collections.Generic;
using MediatR;

namespace OrderingSvc.Application.Orders.Commands;

#region Cart Commands

public record AddToCartCommand(
    Guid CustomerId,
    Guid RestaurantId,
    Guid MenuItemId,
    string MenuItemName,
    decimal UnitPrice,
    int Quantity,
    string? SpecialInstructions) : IRequest;

public record UpdateCartItemCommand(
    Guid CustomerId,
    Guid MenuItemId,
    int Quantity) : IRequest;

public record RemoveFromCartCommand(
    Guid CustomerId,
    Guid MenuItemId) : IRequest;

public record ClearCartCommand(Guid CustomerId) : IRequest;

#endregion

#region Order Commands

public record CreateOrderCommand(
    Guid CustomerId,
    Guid RestaurantId,
    List<OrderItemInput> Items,
    DeliveryAddressInput? DeliveryAddress,
    PaymentMethodType PaymentMethod,
    Guid? PaymentMethodId, // For saved cards
    string? PromoCode,
    string? SpecialInstructions,
    decimal? Tip,
    decimal? DeliveryFee) : IRequest<CreateOrderResult>;

public record DeliveryAddressInput(
    string Street,
    string? BuildingNumber,
    string? Apartment,
    string? Floor,
    string City,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude,
    string? Instructions);

public record OrderItemInput(
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string? SpecialInstructions);

public record CreateOrderResult(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount,
    PaymentResult Payment);

public record PaymentResult(
    bool RequiresAction,
    string? PaymentIntentId,
    string? ClientSecret, // For Stripe
    string? ActionUrl);

#endregion

#region Payment Commands

public record ProcessPaymentCommand(
    Guid OrderId,
    PaymentMethodType PaymentMethod,
    string? CardToken, // For new cards
    Guid? SavedPaymentMethodId) : IRequest<ProcessPaymentResult>;

public record ProcessPaymentResult(
    bool Success,
    string? TransactionId,
    string? ErrorMessage,
    bool RequiresAction,
    string? ActionUrl);

public record ConfirmPaymentCommand(
    Guid OrderId,
    string PaymentIntentId) : IRequest;

public record RefundPaymentCommand(
    Guid OrderId,
    decimal? Amount, // Partial refund if specified
    string Reason) : IRequest<string>; // Returns refund ID

#endregion

#region Order Status Commands

public record ConfirmOrderPaymentCommand(
    Guid OrderId,
    string TransactionId,
    PaymentMethodType PaymentMethod) : IRequest;

public record StartPreparationCommand(Guid OrderId) : IRequest;

public record MarkReadyForPickupCommand(Guid OrderId) : IRequest;

public record AssignRiderCommand(
    Guid OrderId,
    Guid RiderId,
    string RiderName,
    string RiderPhone) : IRequest;

public record MarkPickedUpCommand(Guid OrderId, Guid RiderId) : IRequest;

public record MarkDeliveredCommand(
    Guid OrderId,
    Guid RiderId,
    decimal? CashCollected) : IRequest;

public record CompleteOrderCommand(Guid OrderId) : IRequest;

public record CancelOrderCommand(
    Guid OrderId,
    string Reason,
    Guid? CancelledBy) : IRequest;

public record RateOrderCommand(
    Guid OrderId,
    Guid CustomerId,
    int RestaurantRating,
    int? DeliveryRating,
    string? Review) : IRequest;

#endregion

public enum PaymentMethodType
{
    CashOnDelivery = 0,
    CreditCard = 1,
    DebitCard = 2,
    Wallet = 3
}
