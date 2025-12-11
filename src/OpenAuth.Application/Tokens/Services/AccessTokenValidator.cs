using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Configurations;
using OpenAuth.Application.Tokens.Exceptions;

namespace OpenAuth.Application.Tokens.Services;

public class AccessTokenValidator : IAccessTokenValidator
{
    private readonly ISigningKeyQueryService _keyProvider;
    private readonly IValidationKeyFactory _validationKeyFactory;

    private readonly string _issuer;

    public AccessTokenValidator(IOptions<JwtOptions> options, IValidationKeyFactory validationKeyFactory, ISigningKeyQueryService keyProvider)
    {
        _validationKeyFactory = validationKeyFactory;
        _keyProvider = keyProvider;

        _issuer = options.Value.Issuer;
    }
    
    public async Task<ClaimsPrincipal> ValidateAsync(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        
        var keys = await _keyProvider.GetActiveKeyDataAsync();
        var validationKeys = keys.Select(_validationKeyFactory.Create);
        
        var tokenParams = new TokenValidationParameters
        {
            NameClaimType = JwtRegisteredClaimNames.Sub,
            ValidIssuer = _issuer,
            ValidateAudience = false,
            IssuerSigningKeys = validationKeys,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
        
        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, tokenParams, out _);
        }
        catch (Exception ex)
        {
            throw new InvalidAccessTokenException("Access token validation failed.", ex);
        }
    }
}