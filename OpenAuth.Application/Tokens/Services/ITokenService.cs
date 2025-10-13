using OpenAuth.Application.Tokens.Dtos;

namespace OpenAuth.Application.Tokens.Services;

public interface ITokenService
{
    Task<TokenGenerationResponse> IssueToken(TokenRequest request, CancellationToken ct = default);
}