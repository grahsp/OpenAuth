using OpenAuth.Domain.Clients.Audiences.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Dtos;

public record IssueTokenRequest(
    ClientId ClientId,
    string ClientSecret,
    AudienceName AudienceName,
    Scope[] RequestedScopes
);