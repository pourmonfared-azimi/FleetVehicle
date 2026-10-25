using FleetGraph.Parts.Application.Abstractions;
using FleetGraph.Parts.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetGraph.Parts.Infrastructure.Persistence;

internal sealed class PartRepository(PartsDbContext db) : IPartRepository
{
    public IQueryable<Part> Query() => db.Parts.AsNoTracking().OrderBy(p => p.Id);

    public async Task<IReadOnlyList<Part>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => await db.Parts.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public Task<Part?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken)
        => db.Parts.AsNoTracking().FirstOrDefaultAsync(p => p.PartNumber == partNumber, cancellationToken);
}

internal sealed class StockLevelRepository(PartsDbContext db) : IStockLevelRepository
{
    public Task<StockLevel?> GetByPartIdAsync(int partId, CancellationToken cancellationToken)
        => db.StockLevels.FirstOrDefaultAsync(s => s.PartId == partId, cancellationToken);

    public async Task<IReadOnlyList<StockLevel>> GetByPartIdsAsync(
        IReadOnlyCollection<int> partIds,
        CancellationToken cancellationToken)
        => await db.StockLevels
            .AsNoTracking()
            .Where(s => partIds.Contains(s.PartId))
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
