using AutoMapper;
using OrderingSvc.Application.Contracts.Orders;
using OrderingSvc.Domain.Orders;
using DomainOrderStatus = OrderingSvc.Domain.Orders.OrderStatus;
using ContractOrderStatus = OrderingSvc.Application.Contracts.Orders.OrderStatus;

namespace OrderingSvc.Application;

/// <summary>
/// AutoMapper profile for Ordering service.
/// </summary>
public class OrderingSvcAutoMapperProfile : Profile
{
    public OrderingSvcAutoMapperProfile()
    {
        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (ContractOrderStatus)(int)src.Status))
            .ForMember(dest => dest.PaymentMethod, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<Order, OrderListDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (ContractOrderStatus)(int)src.Status))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // Order item mappings
        CreateMap<OrderItem, OrderItemDto>();

        // Delivery address mappings
        CreateMap<DeliveryAddress, DeliveryAddressDto>();
    }
}
