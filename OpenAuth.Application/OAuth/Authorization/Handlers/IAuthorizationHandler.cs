using OpenAuth.Application.OAuth.Authorization.Dtos;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public interface IAuthorizationHandler
{
    Task<AuthorizationResponse> AuthorizeAsync(AuthorizationRequest request);
}