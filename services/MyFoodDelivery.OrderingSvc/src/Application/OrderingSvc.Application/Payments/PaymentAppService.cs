using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrderingSvc.Application.Contracts.Payments;
using OrderingSvc.Application.Contracts.Payments.Dtos;
using OrderingSvc.Application.Contracts.Permissions;
using OrderingSvc.Domain.Orders;
using OrderingSvc.Domain.Services;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace OrderingSvc.Application.Payments;

/// <summary>
/// ABP Application Service for payment processing.
/// </summary>
[Authorize]
public class PaymentAppService : ApplicationService, IPaymentAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentAppService(
        IRepository<Order, Guid> orderRepository,
        IPaymentGateway paymentGateway)
    {
        _orderRepository = orderRepository;
        _paymentGateway = paymentGateway;
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentDto input)
    {
        var order = await _orderRepository.GetAsync(input.OrderId);
        var userId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");

        if (order.CustomerId != userId)
        {
            throw new BusinessException("Order:NotYours");
        }

        var intent = await _paymentGateway.CreatePaymentIntentAsync(
            order.Id, 
            order.TotalAmount, 
            "usd",
            CurrentUser.Email);

        return new PaymentIntentDto
        {
            PaymentIntentId = intent.PaymentIntentId ?? string.Empty,
            ClientSecret = intent.ClientSecret ?? string.Empty,
            Amount = order.TotalAmount,
            Currency = "usd",
            Status = intent.Success ? "requires_payment_method" : "failed"
        };
    }

    public async Task<PaymentResultDto> ConfirmPaymentAsync(ConfirmPaymentDto input)
    {
        var result = await _paymentGateway.ProcessPaymentAsync(
            input.PaymentIntentId,
            input.PaymentMethodId);

        return new PaymentResultDto
        {
            Success = result.Success,
            TransactionId = result.TransactionId,
            ErrorMessage = result.ErrorMessage,
            ProcessedAt = result.Success ? DateTime.UtcNow : null
        };
    }

    [Authorize(OrderingSvcPermissions.Payments.Refund)]
    public async Task<RefundResultDto> ProcessRefundAsync(ProcessRefundDto input)
    {
        var order = await _orderRepository.GetAsync(input.OrderId);

        var amount = input.Amount ?? order.TotalAmount;
        var result = await _paymentGateway.RefundPaymentAsync(
            order.Id.ToString(), 
            amount, 
            input.Reason ?? "Customer requested refund");

        return new RefundResultDto
        {
            Success = result.Success,
            RefundId = result.RefundId,
            Amount = amount,
            ErrorMessage = result.ErrorMessage
        };
    }

    public Task HandleWebhookAsync(string payload, string signature)
    {
        // TODO: Implement Stripe webhook handling
        return Task.CompletedTask;
    }
}
