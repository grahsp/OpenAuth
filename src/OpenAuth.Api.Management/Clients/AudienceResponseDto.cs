namespace OpenAuth.Api.Management.Clients;

public sealed record AudienceResponseDto(string Name, IEnumerable<string> Scope);