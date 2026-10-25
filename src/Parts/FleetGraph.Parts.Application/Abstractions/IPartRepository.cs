using FleetGraph.Parts.Domain;

namespace FleetGraph.Parts.Application.Abstractions;

public interface IPartRepository
{
    IQueryable<Part> Query();

    Task<IReadOnlyList<Part>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    Task<Part?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken);
}

public interface IStockLevelRepository
{
    /// <summary>Loads the stock row for modification.</summary>
    Task<StockLevel?> GetByPartIdAsync(int partId, CancellationToken cancellationToken);

    Task<IReadOnlyList<StockLevel>> GetByPartIdsAsync(
        IReadOnlyCollection<int> partIds,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
