using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerSvc.Domain.Customers;
using MediatR;
using Volo.Abp;
using Volo.Abp.Uow;

namespace CustomerSvc.Application.Customers.Commands.Handlers;

/// <summary>
/// Handler for AddPaymentMethodCommand.
/// </summary>
public class AddPaymentMethodCommandHandler : IRequestHandler<AddPaymentMethodCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;

    public AddPaymentMethodCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task<Guid> Handle(AddPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithPaymentMethodsAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            customer = new Customer(request.CustomerId);
            await _customerRepository.InsertAsync(customer, autoSave: true, cancellationToken: cancellationToken);
            customer = (await _customerRepository.GetWithPaymentMethodsAsync(request.CustomerId, cancellationToken))!;
        }

        if (!Enum.TryParse<PaymentMethodType>(request.Type, true, out var paymentType))
        {
            throw new BusinessException("Customer:InvalidPaymentMethodType");
        }

        var paymentMethodId = customer.AddPaymentMethod(
            paymentType,
            request.Label,
            request.Last4Digits,
            request.CardBrand,
            request.ExternalToken,
            request.ExpiryDate,
            request.IsDefault);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);

        return paymentMethodId;
    }
}

/// <summary>
/// Handler for RemovePaymentMethodCommand.
/// </summary>
public class RemovePaymentMethodCommandHandler : IRequestHandler<RemovePaymentMethodCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public RemovePaymentMethodCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(RemovePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithPaymentMethodsAsync(request.CustomerId, cancellationToken)
            ?? throw new BusinessException("Customer:NotFound");

        customer.RemovePaymentMethod(request.PaymentMethodId);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for SetDefaultPaymentMethodCommand.
/// </summary>
public class SetDefaultPaymentMethodCommandHandler : IRequestHandler<SetDefaultPaymentMethodCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public SetDefaultPaymentMethodCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(SetDefaultPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithPaymentMethodsAsync(request.CustomerId, cancellationToken)
            ?? throw new BusinessException("Customer:NotFound");

        customer.SetDefaultPaymentMethod(request.PaymentMethodId);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for AddLoyaltyPointsCommand.
/// </summary>
public class AddLoyaltyPointsCommandHandler : IRequestHandler<AddLoyaltyPointsCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public AddLoyaltyPointsCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(AddLoyaltyPointsCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetAsync(request.CustomerId, cancellationToken: cancellationToken);

        customer.AddLoyaltyPoints(request.Points, request.Reason);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for DeductLoyaltyPointsCommand.
/// </summary>
public class DeductLoyaltyPointsCommandHandler : IRequestHandler<DeductLoyaltyPointsCommand, bool>
{
    private readonly ICustomerRepository _customerRepository;

    public DeductLoyaltyPointsCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task<bool> Handle(DeductLoyaltyPointsCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetAsync(request.CustomerId, cancellationToken: cancellationToken);

        var success = customer.TryDeductLoyaltyPoints(request.Points, request.OrderId);

        if (success)
        {
            await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
        }

        return success;
    }
}

/// <summary>
/// Handler for DeactivateCustomerCommand.
/// </summary>
public class DeactivateCustomerCommandHandler : IRequestHandler<DeactivateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public DeactivateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetAsync(request.CustomerId, cancellationToken: cancellationToken);

        customer.Deactivate(request.Reason);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for ReactivateCustomerCommand.
/// </summary>
public class ReactivateCustomerCommandHandler : IRequestHandler<ReactivateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public ReactivateCustomerCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(ReactivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetAsync(request.CustomerId, cancellationToken: cancellationToken);

        customer.Reactivate();

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}
