namespace OpenAuth.Application.Tokens.Flows;

public interface IAuthorizationCodeValidator
{
    Task<AuthorizationCodeValidationResult>  ValidateAsync(AuthorizationCodeValidatorContext context, CancellationToken ct = default);
}