using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Infrastructure.Persistence;

/// <summary>
/// Demo data. Vehicle ids 1-8 come from the Vehicles seed, part ids 1-10 from the Parts seed.
/// </summary>
internal static class ServiceSeed
{
    public static IReadOnlyList<Workshop> Workshops() =>
    [
        Workshop.Create("Eskilstuna Truck Center", "Eskilstuna", 6),
        Workshop.Create("Göteborg Heavy Service", "Göteborg", 10),
        Workshop.Create("Västerås Fleet Care", "Västerås", 4)
    ];

    public static IEnumerable<ServiceOrder> ServiceOrders(DateTimeOffset now)
    {
        ServiceOrder Order(
            int vehicleId,
            int workshopId,
            string description,
            int inDays,
            params (int PartId, int Quantity)[] lines)
            => ServiceOrder.Create(vehicleId, workshopId, description, now.AddDays(inDays), lines, now);

        var o1 = Order(1, 1, "Annual inspection and oil change", 1, (1, 1), (2, 12));
        var o2 = Order(1, 1, "Replace front brake pads", 3, (3, 2));
        var o3 = Order(2, 2, "Clutch replacement", 2, (4, 1), (2, 4));
        var o4 = Order(3, 2, "Tachograph calibration", 5);
        var o5 = Order(4, 3, "AdBlue system fault", 1, (5, 1), (6, 2));
        var o6 = Order(5, 1, "High voltage battery check", 7, (7, 1));
        var o7 = Order(6, 3, "Air dryer cartridge replacement", 2, (8, 1));
        var o8 = Order(2, 2, "Headlight alignment", 4, (9, 2));
        var o9 = Order(7, 1, "Turbocharger inspection", 6, (10, 1), (2, 6));

        o1.ChangeStatus(ServiceOrderStatus.InProgress, now);
        o3.ChangeStatus(ServiceOrderStatus.InProgress, now);
        o3.ChangeStatus(ServiceOrderStatus.AwaitingParts, now);
        o5.ChangeStatus(ServiceOrderStatus.InProgress, now);
        o5.ChangeStatus(ServiceOrderStatus.Completed, now);
        o7.ChangeStatus(ServiceOrderStatus.Cancelled, now);

        return [o1, o2, o3, o4, o5, o6, o7, o8, o9];
    }
}
