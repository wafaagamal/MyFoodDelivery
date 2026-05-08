using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RestaurantSvc.Application.Restaurants.Commands;
using RestaurantSvc.Domain.Restaurants;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using MyFoodDelivery.Shared.Events;

namespace RestaurantSvc.Application.Restaurants.Handlers;

public class RestaurantCommandHandlers :
    IRequestHandler<RegisterRestaurantCommand, Guid>,
    IRequestHandler<UpdateRestaurantInfoCommand>,
    IRequestHandler<OpenRestaurantCommand>,
    IRequestHandler<CloseRestaurantCommand>,
    IRequestHandler<AddCategoryCommand, Guid>,
    IRequestHandler<AddMenuItemCommand, Guid>,
    IRequestHandler<UpdateMenuItemCommand>,
    IRequestHandler<SetMenuItemAvailabilityCommand>,
    IRequestHandler<ConfirmOrderCommand>,
    IRequestHandler<RejectOrderCommand>,
    IRequestHandler<StartPreparingOrderCommand>,
    IRequestHandler<MarkOrderReadyCommand>
{
    private readonly IRepository<Restaurant, Guid> _repository;
    private readonly IDistributedEventBus _eventBus;

    public RestaurantCommandHandlers(
        IRepository<Restaurant, Guid> repository,
        IDistributedEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(RegisterRestaurantCommand request, CancellationToken cancellationToken)
    {
        var address = new RestaurantAddress(
            request.Street,
            request.BuildingNumber,
            request.City,
            request.District,
            request.PostalCode,
            request.Country,
            request.Latitude,
            request.Longitude);

        var restaurant = new Restaurant(
            Guid.NewGuid(),
            request.OwnerId,
            request.Name,
            request.Description,
            request.CuisineType,
            request.PhoneNumber,
            request.Email,
            address,
            request.MinimumOrderAmount,
            request.DeliveryFee,
            request.EstimatedDeliveryMinutes);

        await _repository.InsertAsync(restaurant, cancellationToken: cancellationToken);

        return restaurant.Id;
    }

    public async Task Handle(UpdateRestaurantInfoCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);

        restaurant.UpdateInfo(
            request.Name,
            request.Description,
            request.CuisineType,
            request.PhoneNumber,
            request.Email,
            request.MinimumOrderAmount,
            request.DeliveryFee,
            request.EstimatedDeliveryMinutes);

        await _repository.UpdateAsync(restaurant, cancellationToken: cancellationToken);
    }

    public async Task Handle(OpenRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);
        restaurant.Open();
        await _repository.UpdateAsync(restaurant, cancellationToken: cancellationToken);

        await _eventBus.PublishAsync(new RestaurantStatusChangedEto(
            restaurant.Id,
            restaurant.IsOpen,
            restaurant.AcceptingOrders));
    }

    public async Task Handle(CloseRestaurantCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);
        restaurant.Close();
        await _repository.UpdateAsync(restaurant, cancellationToken: cancellationToken);

        await _eventBus.PublishAsync(new RestaurantStatusChangedEto(
            restaurant.Id,
            restaurant.IsOpen,
            restaurant.AcceptingOrders));
    }

    public async Task<Guid> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);

        var categoryId = restaurant.AddCategory(
            request.Name,
            request.Description,
            request.DisplayOrder);

        await _repository.UpdateAsync(restaurant, cancellationToken: cancellationToken);

        return categoryId;
    }

    public async Task<Guid> Handle(AddMenuItemCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);

        var menuItemId = restaurant.AddMenuItem(
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.PreparationTimeMinutes,
            request.IsVegetarian,
            request.IsVegan,
            request.IsGlutenFree,
            request.IsSpicy,
            request.Allergens);

        await _repository.UpdateAsync(restaurant, cancellationToken: cancellationToken);

        return menuItemId;
    }

    public async Task Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);

        restaurant.UpdateMenuItem(
            request.MenuItemId,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.PreparationTimeMinutes,
            request.IsVegetarian,
            request.IsVegan,
            request.IsGlutenFree,
            request.IsSpicy,
            request.Allergens);

        await _repository.UpdateAsync(restaurant, cancellationToken: cancellationToken);
    }

    public async Task Handle(SetMenuItemAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);
        restaurant.SetMenuItemAvailability(request.MenuItemId, request.IsAvailable);
        await _repository.UpdateAsync(restaurant, cancellationToken: cancellationToken);

        if (!request.IsAvailable)
        {
            var menuItem = restaurant.GetMenuItemOrThrow(request.MenuItemId);
            await _eventBus.PublishAsync(new MenuItemUnavailableEto(
                restaurant.Id,
                request.MenuItemId,
                menuItem.Name));
        }
    }

    public async Task Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var restaurant = await _repository.GetAsync(request.RestaurantId, cancellationToken: cancellationToken);

        if (!restaurant.CanAcceptOrders())
            throw new BusinessException("Restaurant:NotAcceptingOrders");

        await _eventBus.PublishAsync(new RestaurantConfirmedOrderEto(
            request.OrderId,
            request.RestaurantId,
            request.EstimatedPreparationMinutes));
    }

    public async Task Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new RestaurantRejectedOrderEto(
            request.OrderId,
            request.RestaurantId,
            request.Reason));
    }

    public async Task Handle(StartPreparingOrderCommand request, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new RestaurantStartedPreparingEto(
            request.OrderId,
            request.RestaurantId,
            DateTime.UtcNow));
    }

    public async Task Handle(MarkOrderReadyCommand request, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(new RestaurantOrderReadyEto(
            request.OrderId,
            request.RestaurantId,
            DateTime.UtcNow));
    }
}
