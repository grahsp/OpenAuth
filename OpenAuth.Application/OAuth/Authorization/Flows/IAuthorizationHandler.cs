namespace OpenAuth.Application.OAuth.Authorization.Flows;

public interface IAuthorizationHandler
{
    Task<AuthorizationResponse> AuthorizeAsync(AuthorizationRequest request);
}