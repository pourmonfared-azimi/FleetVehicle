using FleetGraph.SharedKernel;

namespace FleetGraph.Service.Domain;

public sealed class Workshop : Entity
{
    private Workshop()
    {
    }

    public string Name { get; private set; } = default!;

    public string City { get; private set; } = default!;

    /// <summary>How many vehicles the workshop can handle at the same time.</summary>
    public int Bays { get; private set; }

    public static Workshop Create(string name, string city, int bays)
        => new()
        {
            Name = Guard.NotEmpty(name, nameof(name), 100),
            City = Guard.NotEmpty(city, nameof(city), 60),
            Bays = Guard.InRange(bays, 1, 100, nameof(bays))
        };
}
