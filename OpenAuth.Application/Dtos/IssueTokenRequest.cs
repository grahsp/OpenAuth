using OpenAuth.Domain.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record IssueTokenRequest(
    ClientId ClientId,
    string ClientSecret,
    AudienceName AudienceName,
    Scope[] RequestedScopes
);