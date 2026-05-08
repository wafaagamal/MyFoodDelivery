namespace OrderingSvc.Application.Contracts.Permissions;

/// <summary>
/// Ordering service permission definitions
/// </summary>
public static class OrderingSvcPermissions
{
    public const string GroupName = "OrderingSvc";

    public static class Orders
    {
        public const string Default = GroupName + ".Orders";
        public const string Create = Default + ".Create";
        public const string Cancel = Default + ".Cancel";
        public const string ViewAll = Default + ".ViewAll";
    }

    public static class RestaurantOrders
    {
        public const string Default = GroupName + ".RestaurantOrders";
        public const string View = Default + ".View";
        public const string Accept = Default + ".Accept";
        public const string Reject = Default + ".Reject";
        public const string UpdateStatus = Default + ".UpdateStatus";
    }

    public static class Payments
    {
        public const string Default = GroupName + ".Payments";
        public const string Process = Default + ".Process";
        public const string Refund = Default + ".Refund";
    }

    public static class Admin
    {
        public const string Default = GroupName + ".Admin";
        public const string ViewAllOrders = Default + ".ViewAllOrders";
        public const string ManageDisputes = Default + ".ManageDisputes";
    }
}
