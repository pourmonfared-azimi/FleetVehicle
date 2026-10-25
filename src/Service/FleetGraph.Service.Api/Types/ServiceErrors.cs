using FleetGraph.Service.Domain;
using FleetGraph.SharedKernel;

namespace FleetGraph.Service.Api.Types;

public sealed class WorkshopNotFoundError : IUserError
{
    private WorkshopNotFoundError(int workshopId, string code, string message)
        => (WorkshopId, Code, Message) = (workshopId, code, message);

    public int WorkshopId { get; }

    public string Code { get; }

    public string Message { get; }

    public static WorkshopNotFoundError CreateErrorFrom(EntityNotFoundException ex)
        => new(ex.EntityId, ex.Code, ex.Message);
}

public sealed class ServiceOrderNotFoundError : IUserError
{
    private ServiceOrderNotFoundError(int serviceOrderId, string code, string message)
        => (ServiceOrderId, Code, Message) = (serviceOrderId, code, message);

    public int ServiceOrderId { get; }

    public string Code { get; }

    public string Message { get; }

    public static ServiceOrderNotFoundError CreateErrorFrom(EntityNotFoundException ex)
        => new(ex.EntityId, ex.Code, ex.Message);
}

public sealed class ServiceOrderValidationError : IUserError
{
    private ServiceOrderValidationError(string field, string code, string message)
        => (Field, Code, Message) = (field, code, message);

    [GraphQLDescription("The input field that failed validation.")]
    public string Field { get; }

    public string Code { get; }

    public string Message { get; }

    public static ServiceOrderValidationError CreateErrorFrom(DomainValidationException ex)
        => new(ex.Field, ex.Code, ex.Message);
}

public sealed class InvalidServiceOrderStatusTransitionError : IUserError
{
    private InvalidServiceOrderStatusTransitionError(
        ServiceOrderStatus from,
        ServiceOrderStatus to,
        string code,
        string message)
        => (From, To, Code, Message) = (from, to, code, message);

    public ServiceOrderStatus From { get; }

    public ServiceOrderStatus To { get; }

    public string Code { get; }

    public string Message { get; }

    public static InvalidServiceOrderStatusTransitionError CreateErrorFrom(
        InvalidServiceOrderStatusTransitionException ex)
        => new(ex.From, ex.To, ex.Code, ex.Message);
}
