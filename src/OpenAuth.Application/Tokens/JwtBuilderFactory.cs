using Microsoft.Extensions.Options;
using OpenAuth.Application.Tokens.Builders;
using OpenAuth.Application.Tokens.Configurations;

namespace OpenAuth.Application.Tokens;

public class JwtBuilderFactory : IJwtBuilderFactory
{
    private string _issuer;
    private readonly TimeProvider _time;

    public JwtBuilderFactory(IOptions<JwtOptions> options, TimeProvider time)
    {
        var opts = options.Value;
        _issuer = opts.Issuer;
        
        _time = time;
    }
    
    public JwtBuilder Create() =>
        new(_issuer, _time);
}