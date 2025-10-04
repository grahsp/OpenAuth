namespace OpenAuth.Application.Dtos;

public record AudienceInfo(string Name, IEnumerable<string> Scopes);