using AutoMapper;
using OrderingSvc.Application.Contracts.Orders;
using OrderingSvc.Application.Contracts.Orders.Dtos;
using OrderingSvc.Domain.Orders;

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
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.SubTotal))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => (OrderingSvc.Application.Contracts.Orders.Dtos.PaymentMethod)src.PaymentMethod))
            .ForMember(dest => dest.PaymentMethodId, opt => opt.MapFrom(src => src.PaymentMethodId))
            .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Tip, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerPhone, opt => opt.Ignore())
            .ForMember(dest => dest.RestaurantName, opt => opt.Ignore())
            .ForMember(dest => dest.PromoCode, opt => opt.Ignore())
            .ForMember(dest => dest.ActualDeliveryTime, opt => opt.Ignore())
            .ForMember(dest => dest.Rating, opt => opt.Ignore())
            .ForMember(dest => dest.Review, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<Order, OrderListDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.RestaurantName, opt => opt.MapFrom(src => src.RestaurantName ?? "Restaurant"))
            .ForMember(dest => dest.RestaurantLogoUrl, opt => opt.Ignore());

        // Order item mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

        // Delivery address mappings
        CreateMap<DeliveryAddress, DeliveryAddressDto>()
            .ForMember(dest => dest.District, opt => opt.Ignore())
            .ForMember(dest => dest.Landmark, opt => opt.Ignore());
    }
}
