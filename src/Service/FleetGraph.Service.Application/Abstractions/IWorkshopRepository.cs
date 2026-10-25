using FleetGraph.Service.Domain;

namespace FleetGraph.Service.Application.Abstractions;

public interface IWorkshopRepository
{
    IQueryable<Workshop> Query();

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Workshop>> GetByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken);
}
