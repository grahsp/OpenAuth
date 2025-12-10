using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public sealed record AuthorizationCodeValidationResult(
    AudienceName? AudienceName,
    ScopeCollection Scope,
    ScopeCollection OidcScope
);