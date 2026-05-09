using System.Collections.Generic;

namespace CustomerSvc.Application.Contracts.Customers.Dtos;

public record CustomerPagedResultDto<T>(
    List<T> Items,
    int TotalCount);
