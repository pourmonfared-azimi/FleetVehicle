namespace FleetGraph.SharedKernel;

/// <summary>
/// Base class for all business rule violations. These are expected errors: the GraphQL layer
/// maps them to typed errors on mutation payloads instead of generic GraphQL errors.
/// </summary>
public abstract class DomainException(string code, string message) : Exception(message)
{
    /// <summary>A stable, machine readable error code (e.g. <c>VEHICLE_NOT_FOUND</c>).</summary>
    public string Code { get; } = code;
}

/// <summary>Raised when an aggregate referenced by id does not exist.</summary>
public sealed class EntityNotFoundException(string entityName, int id)
    : DomainException(
        $"{ToCode(entityName)}_NOT_FOUND",
        $"{entityName} with id {id} was not found.")
{
    public string EntityName { get; } = entityName;

    public int EntityId { get; } = id;

    private static string ToCode(string entityName)
        => string.Concat(entityName.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + c : c.ToString())).ToUpperInvariant();
}

/// <summary>Raised when input does not satisfy a domain invariant.</summary>
public sealed class DomainValidationException(string field, string message)
    : DomainException("VALIDATION_FAILED", message)
{
    public string Field { get; } = field;
}
