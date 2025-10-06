namespace OpenAuth.Domain.Clients.Audiences.ValueObjects;

public record AudienceId(Guid Value)
{
    public static AudienceId New()
        => new(Guid.NewGuid());
}