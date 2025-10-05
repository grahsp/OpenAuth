namespace OpenAuth.Domain.ValueObjects;

public record AudienceId(Guid Value)
{
    public static AudienceId New()
        => new(Guid.NewGuid());
}