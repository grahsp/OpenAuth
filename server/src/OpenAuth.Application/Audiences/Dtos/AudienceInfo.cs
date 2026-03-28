using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Audiences.Dtos;

public record AudienceInfo(AudienceName Name, IEnumerable<Scope> Scopes);