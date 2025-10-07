using OpenAuth.Application.Tokens.Dtos;

namespace OpenAuth.Application.Tokens.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(TokenGenerationRequest request);
}