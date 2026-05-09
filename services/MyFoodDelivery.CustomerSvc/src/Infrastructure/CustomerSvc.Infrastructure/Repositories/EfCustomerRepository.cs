using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerSvc.Domain.Customers;
using CustomerSvc.Domain.Customers.Events;
using CustomerSvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CustomerSvc.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ICustomerRepository.
/// No email/name queries — identity data lives in AuthSvc.
/// </summary>
public class EfCustomerRepository : EfCoreRepository<CustomerSvcDbContext, Customer, Guid>, ICustomerRepository
{
    public EfCustomerRepository(IDbContextProvider<CustomerSvcDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
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
}
