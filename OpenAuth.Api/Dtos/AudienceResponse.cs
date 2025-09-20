namespace OpenAuth.Api.Dtos;

public record AudienceResponse(string Audience, IEnumerable<string> Scopes);