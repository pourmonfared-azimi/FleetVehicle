namespace FleetGraph.ServiceDefaults;

/// <summary>Role and policy names shared by the gateway, the subgraphs and the tests.</summary>
public static class FleetGraphAuth
{
    public static class Roles
    {
        /// <summary>May see owner contact data on vehicles.</summary>
        public const string FleetManager = "FleetManager";

        /// <summary>May create and progress service orders.</summary>
        public const string Technician = "Technician";

        /// <summary>May change stock levels.</summary>
        public const string Warehouse = "Warehouse";
    }

    public static class Policies
    {
        public const string CanViewOwnerData = "CanViewOwnerData";
        public const string CanManageStock = "CanManageStock";
        public const string CanManageServiceOrders = "CanManageServiceOrders";
    }

    public const string RoleClaimType = "role";
    public const string NameClaimType = "sub";
}
