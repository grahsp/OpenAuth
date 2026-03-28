using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public sealed record AuthorizationCodeValidationResult(
    ScopeCollection Scope,
    ScopeCollection OidcScope
);