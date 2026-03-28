using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Application.Tokens.Flows;

public record AuthorizationCodeValidatorContext(
    AuthorizationCodeTokenCommand Command,
    ClientTokenData TokenData,
    AuthorizationGrant AuthorizationGrant
);