using FleetGraph.Parts.Domain;

namespace FleetGraph.Parts.Infrastructure.Persistence;

/// <summary>Demo catalog. Ids 1-10 are referenced by the Service seed.</summary>
internal static class PartsSeed
{
    public static IReadOnlyList<(Part Part, int OnHand, int ReorderAt)> Create() =>
    [
        (Part.Create("OF-2210", "Oil filter", PartCategory.Filters, 189m), 42, 10),
        (Part.Create("EO-5W30-1L", "Engine oil 5W-30 (1 L)", PartCategory.Fluids, 95m), 300, 60),
        (Part.Create("BP-FR-880", "Brake pad set, front axle", PartCategory.Brakes, 2_450m), 6, 4),
        (Part.Create("CL-KIT-430", "Clutch kit 430 mm", PartCategory.Drivetrain, 14_900m), 1, 1),
        (Part.Create("AB-PUMP-02", "AdBlue dosing pump", PartCategory.Exhaust, 6_300m), 3, 2),
        (Part.Create("AB-NOZ-11", "AdBlue injector nozzle", PartCategory.Exhaust, 1_750m), 8, 3),
        (Part.Create("HV-FUSE-400", "High voltage fuse 400 A", PartCategory.Electrical, 3_200m), 5, 2),
        (Part.Create("AD-CART-07", "Air dryer cartridge", PartCategory.Filters, 980m), 12, 5),
        (Part.Create("HL-LED-R", "LED headlamp, right", PartCategory.Lighting, 7_800m), 2, 2),
        (Part.Create("TC-VGT-13", "Turbocharger VGT 13 L", PartCategory.Engine, 32_500m), 0, 1)
    ];
}
