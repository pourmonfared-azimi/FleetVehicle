using FleetGraph.Parts.Application.Abstractions;
using FleetGraph.Parts.Domain;

namespace FleetGraph.Parts.Application.Queries;

public sealed class PartReadService(IPartRepository parts, IStockLevelRepository stock)
{
    public IQueryable<Part> Search() => parts.Query();

    public Task<Part?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken)
        => parts.GetByPartNumberAsync(partNumber.Trim().ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyDictionary<int, Part>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => (await parts.GetByIdsAsync(ids, cancellationToken)).ToDictionary(p => p.Id);

    public async Task<IReadOnlyDictionary<int, StockLevel>> GetStockByPartIdsAsync(
        IReadOnlyCollection<int> partIds,
        CancellationToken cancellationToken)
        => (await stock.GetByPartIdsAsync(partIds, cancellationToken)).ToDictionary(s => s.PartId);
}
