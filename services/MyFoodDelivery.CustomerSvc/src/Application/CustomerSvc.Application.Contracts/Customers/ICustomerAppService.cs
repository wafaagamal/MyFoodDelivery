using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerSvc.Application.Contracts.Customers.Dtos;

namespace CustomerSvc.Application.Contracts.Customers;

public interface ICustomerAppService
{
    Task<CustomerProfileDto?> GetProfileAsync(Guid customerId);
    Task<List<DeliveryAddressDto>> GetAddressesAsync(Guid customerId);
    Task<DeliveryAddressDto?> GetAddressAsync(Guid customerId, Guid addressId);
    Task<List<PaymentMethodDto>> GetPaymentMethodsAsync(Guid customerId);
    Task<LoyaltyInfoDto?> GetLoyaltyInfoAsync(Guid customerId);
    Task<CustomerPagedResultDto<CustomerListItemDto>> SearchCustomersAsync(
        string? searchTerm, string? loyaltyTier, bool? isActive, int skipCount = 0, int maxResultCount = 20);
}
