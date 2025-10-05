using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record AudienceInfo(AudienceName Name, IEnumerable<Scope> Scopes);