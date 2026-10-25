using FleetGraph.Service.Api.Types;
using FleetGraph.Service.Application.Abstractions;
using FleetGraph.Service.Application.Contracts;

namespace FleetGraph.Service.Api;

/// <summary>
/// Adapter from the application's outbound port to Hot Chocolate's topic sender
/// (in-memory or Redis, depending on configuration).
/// </summary>
internal sealed class SubscriptionEventPublisher(ITopicEventSender sender) : IServiceOrderEventPublisher
{
    public ValueTask PublishStatusChangedAsync(
        ServiceOrderStatusChanged @event,
        CancellationToken cancellationToken)
        => sender.SendAsync(ServiceSubscriptions.StatusChangedTopic, @event, cancellationToken);
}
