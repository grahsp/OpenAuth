using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Application.OAuth.Authorization.Handlers;

public class AuthorizationHandler : IAuthorizationHandler
{
    private readonly IAuthorizationRequestValidator _validator;
    private readonly IAuthorizationGrantStore _store;
    private readonly TimeProvider _time;
    
    public AuthorizationHandler(
        IAuthorizationRequestValidator validator,
        IAuthorizationGrantStore store,
        TimeProvider time)
    {
        _validator = validator;
        _store = store;
        _time = time;
    }

    public async Task<AuthorizationGrant> HandleAsync(AuthorizeCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var result = await _validator.ValidateAsync(command, ct);

        var code = GenerateCode();
        
        var authorizationGrant = AuthorizationGrant.Create(
            code,
            GrantType.AuthorizationCode,
            command.Subject,
            result.ClientId,
            result.RedirectUri,
            result.Scopes,
            result.Pkce,
            result.Nonce,
            _time.GetUtcNow()
        );
        
        await _store.AddAsync(authorizationGrant);
        return authorizationGrant;
    }

    private static string GenerateCode()
        => Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
}