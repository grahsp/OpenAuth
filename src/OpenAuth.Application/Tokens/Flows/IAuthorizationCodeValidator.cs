using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Domain.AuthorizationGrants;

namespace OpenAuth.Application.Tokens.Flows;

public interface IAuthorizationCodeValidator
{
    Task<AuthorizationCodeValidationResult>  ValidateAsync(AuthorizationCodeTokenCommand command, ClientTokenData tokenData, AuthorizationGrant authorizationGrant, CancellationToken ct = default);
}