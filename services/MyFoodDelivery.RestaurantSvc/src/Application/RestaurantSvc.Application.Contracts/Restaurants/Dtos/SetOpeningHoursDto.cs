using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class SetOpeningHoursDto
{
    [Required]
    public List<OpeningHoursDto> Hours { get; set; } = new();
}
