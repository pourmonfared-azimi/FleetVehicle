using System.Linq.Expressions;
using FleetGraph.Vehicles.Domain;

namespace FleetGraph.Vehicles.Api.Types;

/// <summary>
/// The GraphQL contract for a vehicle. Kept separate from the domain entity so the schema can
/// evolve independently of the domain model (and domain methods never leak into the schema).
/// </summary>
[GraphQLName("Vehicle")]
[GraphQLDescription("A vehicle in the fleet. Owning subgraph: Vehicles.")]
public sealed class VehicleNode
{
    public int Id { get; init; }

    [GraphQLDescription("Vehicle Identification Number (17 characters).")]
    public required string Vin { get; init; }

    public required string RegistrationNumber { get; init; }

    public required string Manufacturer { get; init; }

    public required string Model { get; init; }

    public int ModelYear { get; init; }

    public int MileageKm { get; init; }

    public VehicleStatus Status { get; init; }

    [Authorize(Policy = FleetGraphAuth.Policies.CanViewOwnerData)]
    [GraphQLDescription("Owner contact (personal data). Requires the FleetManager role.")]
    public string? OwnerEmail { get; init; }

    public DateTimeOffset RegisteredAt { get; init; }
}

public static class VehicleMapping
{
    /// <summary>Used inside IQueryable pipelines so EF Core translates it to SQL.</summary>
    public static readonly Expression<Func<Vehicle, VehicleNode>> Projection = v => new VehicleNode
    {
        Id = v.Id,
        Vin = v.Vin,
        RegistrationNumber = v.RegistrationNumber,
        Manufacturer = v.Manufacturer,
        Model = v.Model,
        ModelYear = v.ModelYear,
        MileageKm = v.MileageKm,
        Status = v.Status,
        OwnerEmail = v.OwnerEmail,
        RegisteredAt = v.RegisteredAt
    };

    private static readonly Func<Vehicle, VehicleNode> s_map = Projection.Compile();

    public static VehicleNode ToNode(this Vehicle vehicle) => s_map(vehicle);

    public static IQueryable<VehicleNode> ToNodes(this IQueryable<Vehicle> query)
        => query.Select(Projection);
}
