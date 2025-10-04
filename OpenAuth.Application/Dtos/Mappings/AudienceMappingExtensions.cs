using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos.Mappings;

public static class AudienceMappingExtensions
{
    public static AudienceInfo ToAudienceInfo(this Audience audience)
        => new AudienceInfo(
            audience.Value,
            audience.Scopes.Select(s => s.Value)
        );
}