using AutoMapper;
using RestaurantSvc.Application.Contracts.Restaurants;
using RestaurantSvc.Application.Contracts.Restaurants.Dtos;
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
                src.OpeningHours.Select(h => new OpeningHoursDto(
                    h.Day,
                    h.OpenTime,
                    h.CloseTime,
                    h.IsClosed)).ToList()))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src =>
                src.Categories.Where(c => c.IsActive).Select(c => new MenuCategoryDto(
                    c.Id,
                    c.Name,
                    c.Description,
                    c.DisplayOrder,
                    src.MenuItems
                        .Where(m => m.CategoryId == c.Id && m.IsActive)
                        .Select(m => new MenuItemDto(
                            m.Id,
                            m.CategoryId,
                            m.Name,
                            m.Description,
                            m.Price,
                            m.DiscountedPrice,
                            m.ImageUrl,
                            m.PreparationTimeMinutes,
                            m.IsVegetarian,
                            m.IsVegan,
                            m.IsGlutenFree,
                            m.IsSpicy,
                            m.Allergens,
                            m.IsAvailable,
                            m.IsFeatured)).ToList()
                )).ToList()));

        // Note: RestaurantListDto, NearbyRestaurantDto and MenuItem/Category records use
        // manual mapping in RestaurantAppService (records with positional constructors).
        CreateMap<RestaurantAddress, RestaurantAddressDto>()
            .ConstructUsing(src => new RestaurantAddressDto(
                src.Street, src.BuildingNumber, src.City, src.District,
                src.PostalCode, src.Country, src.Latitude, src.Longitude,
                src.GetFullAddress()));
    }
}
