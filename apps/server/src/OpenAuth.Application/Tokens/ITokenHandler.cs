namespace OpenAuth.Application.Tokens;

public interface ITokenHandler<in TContext>
{
    Task<string> CreateAsync(TContext context, CancellationToken ct = default);
}