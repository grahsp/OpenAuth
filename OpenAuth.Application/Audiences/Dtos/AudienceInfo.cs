using OpenAuth.Domain.Clients.Audiences.ValueObjects;

namespace OpenAuth.Application.Audiences.Dtos;

public record AudienceInfo(AudienceName Name, IEnumerable<Scope> Scopes);