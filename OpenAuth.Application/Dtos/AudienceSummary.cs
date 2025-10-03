namespace OpenAuth.Application.Dtos;

public record AudienceSummary(string Name, IEnumerable<string> Scopes);