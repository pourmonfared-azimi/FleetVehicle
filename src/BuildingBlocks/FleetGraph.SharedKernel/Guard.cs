namespace FleetGraph.SharedKernel;

/// <summary>Small helpers that keep invariant checks in entities readable.</summary>
public static class Guard
{
    public static string NotEmpty(string? value, string field, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException(field, $"{field} is required.");
        }

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new DomainValidationException(
                field,
                $"{field} must be at most {maxLength} characters.");
        }

        return trimmed;
    }

    public static int InRange(int value, int min, int max, string field)
    {
        if (value < min || value > max)
        {
            throw new DomainValidationException(
                field,
                $"{field} must be between {min} and {max}.");
        }

        return value;
    }

    public static int Positive(int value, string field)
    {
        if (value <= 0)
        {
            throw new DomainValidationException(field, $"{field} must be greater than zero.");
        }

        return value;
    }

    public static decimal NotNegative(decimal value, string field)
    {
        if (value < 0)
        {
            throw new DomainValidationException(field, $"{field} must not be negative.");
        }

        return value;
    }
}
