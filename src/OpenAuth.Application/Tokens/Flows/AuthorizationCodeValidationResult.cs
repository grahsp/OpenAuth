using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.Tokens.Flows;

public sealed record AuthorizationCodeValidationResult(AuthorizationGrant AuthorizationGrant, ClientTokenData TokenData, Audience Audience);