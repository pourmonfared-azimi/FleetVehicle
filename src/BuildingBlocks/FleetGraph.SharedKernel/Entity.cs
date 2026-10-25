namespace FleetGraph.SharedKernel;

/// <summary>
/// Base type for aggregate roots and entities. The identity is a database generated integer,
/// which is also the federation key (<c>id</c>) used across subgraphs.
/// </summary>
public abstract class Entity
{
    public int Id { get; protected set; }
}
