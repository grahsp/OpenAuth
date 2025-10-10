using OpenAuth.Application.Audiences.Dtos;
using OpenAuth.Domain.Clients.Audiences;

namespace OpenAuth.Application.Audiences.Mappings;

public static class AudienceMappingExtensions
{
    public static AudienceInfo ToAudienceInfo(this Audience audience)
        => new AudienceInfo(
            audience.Name,
            audience.AllowedScopes
        );
}