namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public interface IAuthorizationRequestValidator
{
    Task<AuthorizationValidationResult> ValidateAsync(AuthorizeCommand command, CancellationToken ct);
}