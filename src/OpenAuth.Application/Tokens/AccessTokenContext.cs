using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens;

public sealed record AccessTokenContext(ClientTokenData TokenData, string Subject, ScopeCollection ApiScopes, AudienceName Audience);