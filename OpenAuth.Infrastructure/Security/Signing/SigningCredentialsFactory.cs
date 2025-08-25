using Microsoft.IdentityModel.Tokens;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Domain.Entities;
using OpenAuth.Domain.Enums;

namespace OpenAuth.Infrastructure.Security.Signing;

public class SigningCredentialsFactory : ISigningCredentialsFactory
{
    private readonly IDictionary<SigningAlgorithm, ISigningCredentialsStrategy> _strategies;
    
    public SigningCredentialsFactory(IEnumerable<ISigningCredentialsStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s =>  s.Algorithm);
    }
    
    public SigningCredentials Create(SigningKey signingKey)
    {
        if (!_strategies.TryGetValue(signingKey.Algorithm, out var strategy))
            throw new InvalidOperationException($"Unknown algorithm: { signingKey.Algorithm }");

        return strategy.GetSigningCredentials(signingKey);
    }
}