using OpenAuth.Application.Tokens.Dtos;

namespace OpenAuth.Application.Tokens.Services;

public interface ITokenService
{
    Task<TokenGenerationResponse> IssueToken(IssueTokenRequest request, CancellationToken ct = default);
}