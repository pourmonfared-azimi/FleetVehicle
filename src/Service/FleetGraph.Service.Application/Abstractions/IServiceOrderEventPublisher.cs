using FleetGraph.Service.Application.Contracts;

namespace FleetGraph.Service.Application.Abstractions;

/// <summary>
/// Outbound port for domain notifications. The API layer implements it on top of the
/// GraphQL subscription provider (in-memory or Redis).
/// </summary>
public interface IServiceOrderEventPublisher
{
    ValueTask PublishStatusChangedAsync(
        ServiceOrderStatusChanged @event,
        CancellationToken cancellationToken);
}
