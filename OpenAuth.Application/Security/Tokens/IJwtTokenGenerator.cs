using OpenAuth.Application.Dtos;

namespace OpenAuth.Application.Security.Tokens;

public interface IJwtTokenGenerator
{
    string GenerateToken(TokenGenerationRequest request);
}