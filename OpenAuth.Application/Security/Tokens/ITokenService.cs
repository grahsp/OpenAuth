using OpenAuth.Application.Dtos;

namespace OpenAuth.Application.Security.Tokens;

public interface ITokenService
{
    Task<TokenGenerationResponse> IssueToken(IssueTokenRequest request, CancellationToken ct = default);
}