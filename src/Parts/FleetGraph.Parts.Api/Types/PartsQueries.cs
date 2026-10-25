using FleetGraph.Parts.Application.Queries;

namespace FleetGraph.Parts.Api.Types;

[QueryType]
public static partial class PartsQueries
{
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    [GraphQLDescription("The parts catalog with cursor pagination, filtering and sorting.")]
    public static IQueryable<PartNode> GetParts(PartReadService parts)
        => parts.Search().ToNodes();

    [Lookup]
    public static async Task<PartNode?> GetPartByIdAsync(
        int id,
        IPartByIdDataLoader partById,
        CancellationToken cancellationToken)
        => await partById.LoadAsync(id, cancellationToken);

    public static async Task<PartNode?> GetPartByPartNumberAsync(
        string partNumber,
        PartReadService parts,
        CancellationToken cancellationToken)
        => (await parts.GetByPartNumberAsync(partNumber, cancellationToken))?.ToNode();
}
