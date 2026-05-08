using AutoMapper;
using RestaurantSvc.Application.Contracts.Restaurants;
using RestaurantSvc.Domain.Restaurants;

namespace RestaurantSvc.Application;

/// <summary>
/// AutoMapper profile for Restaurant service.
/// </summary>
public class RestaurantSvcAutoMapperProfile : Profile
{
    public RestaurantSvcAutoMapperProfile()
    {
        // Restaurant mappings
        CreateMap<Restaurant, RestaurantDto>()
            .ForMember(dest => dest.OpeningHours, opt => opt.MapFrom(src =>
                src.OpeningHours.Select(h => new OpeningHoursDto
                {
                    Day = h.Day,
                    OpenTime = h.OpenTime,
                    CloseTime = h.CloseTime,
                    IsClosed = h.IsClosed
                }).ToList()))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Categories.Where(c => c.IsActive).Select(c => new MenuCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    Items = src.MenuItems
                        .Where(m => m.CategoryId == c.Id && m.IsActive)
                        .Select(m => new MenuItemDto
                        {
                            Id = m.Id,
                            CategoryId = m.CategoryId,
                            Name = m.Name,
                            Description = m.Description,
                            Price = m.Price,
                            DiscountedPrice = m.DiscountedPrice,
                            ImageUrl = m.ImageUrl,
                            PreparationTimeMinutes = m.PreparationTimeMinutes,
                            IsVegetarian = m.IsVegetarian,
                            IsVegan = m.IsVegan,
                            IsGlutenFree = m.IsGlutenFree,
                            IsSpicy = m.IsSpicy,
                            Allergens = m.Allergens,
                            IsAvailable = m.IsAvailable,
                            IsFeatured = m.IsFeatured
                        }).ToList()
                }).ToList()));

        CreateMap<Restaurant, RestaurantListDto>();

        CreateMap<Restaurant, NearbyRestaurantDto>();

        CreateMap<RestaurantAddress, RestaurantAddressDto>()
            .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => src.GetFullAddress()));

        // Menu item mappings
        CreateMap<MenuItem, MenuItemDto>();

        CreateMap<MenuCategory, MenuCategoryDto>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());
    }
}
