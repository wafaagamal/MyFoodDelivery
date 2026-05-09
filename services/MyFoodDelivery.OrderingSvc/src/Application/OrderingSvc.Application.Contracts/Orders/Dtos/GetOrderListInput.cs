using System;
using OrderingSvc.Domain.Orders;
using Volo.Abp.Application.Dtos;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class GetOrderListInput : PagedAndSortedResultRequestDto
{
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

