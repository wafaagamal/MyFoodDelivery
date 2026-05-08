namespace RestaurantSvc.Application.Contracts.Permissions;

/// <summary>
/// Restaurant service permission definitions
/// </summary>
public static class RestaurantSvcPermissions
{
    public const string GroupName = "RestaurantSvc";

    public static class Restaurants
    {
        public const string Default = GroupName + ".Restaurants";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageMenu = Default + ".ManageMenu";
        public const string ManageOrders = Default + ".ManageOrders";
    }

    public static class Admin
    {
        public const string Default = GroupName + ".Admin";
        public const string ApproveRestaurants = Default + ".ApproveRestaurants";
        public const string SuspendRestaurants = Default + ".SuspendRestaurants";
    }
}
