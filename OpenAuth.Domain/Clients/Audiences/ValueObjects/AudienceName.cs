namespace OpenAuth.Domain.Clients.Audiences.ValueObjects;

public record AudienceName
{
    public string Value { get; }

    public AudienceName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Audience name cannot be empty.", nameof(value));

        var normalized = value.ToLowerInvariant().Trim();
        Value = normalized.Length switch
        {
            < MinLength => throw new ArgumentException($"Audience name must be at least {MinLength} characters.",
                nameof(value)),
            > MaxLength => throw new ArgumentException($"Audience name cannot exceed {MaxLength} characters.",
                nameof(value)),
            _ => normalized
        };
    }

    private const int MinLength = 1;
    private const int MaxLength = 100;
}