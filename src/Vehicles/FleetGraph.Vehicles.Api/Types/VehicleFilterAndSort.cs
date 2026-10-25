using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;

namespace FleetGraph.Vehicles.Api.Types;

/// <summary>
/// Explicit filter contract. Fields are opt-in so personal data (ownerEmail) can never be
/// used to probe values through filters, and the filter surface stays a deliberate API.
/// </summary>
public sealed class VehicleFilterInputType : FilterInputType<VehicleNode>
{
    protected override void Configure(IFilterInputTypeDescriptor<VehicleNode> descriptor)
    {
        descriptor.Name("VehicleFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(v => v.Id);
        descriptor.Field(v => v.Vin);
        descriptor.Field(v => v.RegistrationNumber);
        descriptor.Field(v => v.Manufacturer);
        descriptor.Field(v => v.Model);
        descriptor.Field(v => v.ModelYear);
        descriptor.Field(v => v.MileageKm);
        descriptor.Field(v => v.Status);
    }
}

public sealed class VehicleSortInputType : SortInputType<VehicleNode>
{
    protected override void Configure(ISortInputTypeDescriptor<VehicleNode> descriptor)
    {
        descriptor.Name("VehicleSortInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(v => v.Id);
        descriptor.Field(v => v.RegistrationNumber);
        descriptor.Field(v => v.Manufacturer);
        descriptor.Field(v => v.ModelYear);
        descriptor.Field(v => v.MileageKm);
        descriptor.Field(v => v.RegisteredAt);
    }
}
