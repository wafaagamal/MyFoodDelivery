using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerSvc.Domain.ValueObjects;
using Volo.Abp.Domain.Repositories;

namespace CustomerSvc.Domain.Customers;

/// <summary>
/// Repository interface for Customer aggregate.
/// Follows repository pattern - only aggregate root operations.
/// </summary>
public interface ICustomerRepository : IRepository<Customer, Guid>
{
    /// <summary>
    /// Finds a customer by their email address.
    /// </summary>
    Task<Customer?> FindByEmailAsync(Email email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a customer by their email address (string version).
    /// </summary>
    Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a customer with the given email exists.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer with their addresses loaded.
    /// </summary>
    Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer with their payment methods loaded.
    /// </summary>
    Task<Customer?> GetWithPaymentMethodsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer with all related data loaded.
    /// </summary>
    Task<Customer?> GetWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customers by their loyalty tier.
    /// </summary>
    Task<List<Customer>> GetByLoyaltyTierAsync(
        Events.LoyaltyTier tier,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active customers who haven't ordered in the specified number of days.
    /// </summary>
    Task<List<Customer>> GetInactiveCustomersAsync(
        int daysSinceLastOrder,
        int skipCount = 0,
        int maxResultCount = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches customers by name or email.
    /// </summary>
    Task<List<Customer>> SearchAsync(
        string searchTerm,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default);
}
