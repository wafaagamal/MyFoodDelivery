using RestaurantSvc.Application.Contracts.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace RestaurantSvc.Application.Contracts.Permissions;

public class RestaurantSvcPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var restaurantGroup = context.AddGroup(RestaurantSvcPermissions.GroupName, L("Permission:RestaurantSvc"));

        var restaurantPermission = restaurantGroup.AddPermission(
            RestaurantSvcPermissions.Restaurants.Default, 
            L("Permission:Restaurants"));
        
        restaurantPermission.AddChild(
            RestaurantSvcPermissions.Restaurants.Create, 
            L("Permission:Restaurants.Create"));
        
        restaurantPermission.AddChild(
            RestaurantSvcPermissions.Restaurants.Edit, 
            L("Permission:Restaurants.Edit"));
        
        restaurantPermission.AddChild(
            RestaurantSvcPermissions.Restaurants.Delete, 
            L("Permission:Restaurants.Delete"));
        
        restaurantPermission.AddChild(
            RestaurantSvcPermissions.Restaurants.ManageMenu, 
            L("Permission:Restaurants.ManageMenu"));
        
        restaurantPermission.AddChild(
            RestaurantSvcPermissions.Restaurants.ManageOrders, 
            L("Permission:Restaurants.ManageOrders"));

        var adminPermission = restaurantGroup.AddPermission(
            RestaurantSvcPermissions.Admin.Default, 
            L("Permission:Admin"));
        
        adminPermission.AddChild(
            RestaurantSvcPermissions.Admin.ApproveRestaurants, 
            L("Permission:Admin.ApproveRestaurants"));
        
        adminPermission.AddChild(
            RestaurantSvcPermissions.Admin.SuspendRestaurants, 
            L("Permission:Admin.SuspendRestaurants"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<RestaurantSvcResource>(name);
    }
}

// Placeholder for localization resource
public class RestaurantSvcResource { }
