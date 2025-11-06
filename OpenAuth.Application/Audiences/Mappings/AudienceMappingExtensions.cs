using OpenAuth.Application.Audiences.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Audiences.Mappings;

public static class AudienceMappingExtensions
{
    public static AudienceInfo ToAudienceInfo(this NewAudience audience)
        => new AudienceInfo(
            audience.Name,
            audience.Scopes
        );
}