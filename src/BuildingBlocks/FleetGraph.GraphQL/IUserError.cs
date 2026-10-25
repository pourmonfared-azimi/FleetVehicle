using HotChocolate;

namespace FleetGraph.GraphQL;

/// <summary>
/// The error interface every typed mutation error implements. Clients can always select
/// <c>message</c> and <c>code</c>, and use fragments for error specific fields.
/// </summary>
[GraphQLName("UserError")]
public interface IUserError
{
    string Message { get; }

    string Code { get; }
}
