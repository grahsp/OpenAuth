using OpenAuth.Application.Tokens.Builders;

namespace OpenAuth.Application.Tokens;

public interface IJwtBuilderFactory
{
    JwtBuilder Create();
}