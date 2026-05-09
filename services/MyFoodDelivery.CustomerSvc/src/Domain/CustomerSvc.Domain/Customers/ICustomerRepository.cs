using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace CustomerSvc.Domain.Customers;

/// <summary>
/// Repository interface for Customer aggregate.
/// </summary>
public interface ICustomerRepository : IRepository<Customer, Guid>
{
    /// <summary>Gets a customer with their addresses loaded.</summary>
    Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a customer with their payment methods loaded.</summary>
    Task<Customer?> GetWithPaymentMethodsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a customer with all related data loaded.</summary>
    Task<Customer?> GetWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets customers by their loyalty tier.</summary>
    Task<List<Customer>> GetByLoyaltyTierAsync(
        Events.LoyaltyTier tier,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default);
}
