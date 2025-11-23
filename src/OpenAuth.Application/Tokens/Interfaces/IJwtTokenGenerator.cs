using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.Tokens.Dtos;

namespace OpenAuth.Application.Tokens.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(TokenContext context, ClientTokenData tokenData, SigningKeyData keyData);
}