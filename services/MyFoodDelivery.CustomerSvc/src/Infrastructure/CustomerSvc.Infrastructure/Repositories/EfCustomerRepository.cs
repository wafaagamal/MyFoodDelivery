using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerSvc.Domain.Customers;
using CustomerSvc.Domain.Customers.Events;
using CustomerSvc.Domain.ValueObjects;
using CustomerSvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CustomerSvc.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ICustomerRepository.
/// Follows repository pattern - only aggregate root operations.
/// </summary>
public class EfCustomerRepository : EfCoreRepository<CustomerSvcDbContext, Customer, Guid>, ICustomerRepository
{
    public EfCustomerRepository(IDbContextProvider<CustomerSvcDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Customer?> FindByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await FindByEmailAsync(email.Value, cancellationToken);
    }

    public async Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var normalizedEmail = email.ToLowerInvariant();

        return await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Email.Value == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var normalizedEmail = email.ToLowerInvariant();

        return await dbContext.Customers
            .AnyAsync(c => c.Email.Value == normalizedEmail, cancellationToken);
    }

    public async Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        return await dbContext.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetWithPaymentMethodsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        return await dbContext.Customers
            .Include(c => c.PaymentMethods)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        return await dbContext.Customers
            .Include(c => c.Addresses)
            .Include(c => c.PaymentMethods)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Customer>> GetByLoyaltyTierAsync(
        LoyaltyTier tier,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        return await dbContext.Customers
            .Where(c => c.LoyaltyTier == tier && c.IsActive)
            .OrderByDescending(c => c.LoyaltyPoints)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Customer>> GetInactiveCustomersAsync(
        int daysSinceLastOrder,
        int skipCount = 0,
        int maxResultCount = 100,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysSinceLastOrder);

        return await dbContext.Customers
            .Where(c => c.IsActive && 
                        (c.LastOrderDate == null || c.LastOrderDate < cutoffDate))
            .OrderBy(c => c.LastOrderDate)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Customer>> SearchAsync(
        string searchTerm,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await dbContext.Customers
                .OrderByDescending(c => c.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync(cancellationToken);
        }

        var term = searchTerm.ToLowerInvariant();

        return await dbContext.Customers
            .Where(c => c.Email.Value.Contains(term) ||
                        c.FirstName.ToLower().Contains(term) ||
                        c.LastName.ToLower().Contains(term))
            .OrderByDescending(c => c.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }
}
