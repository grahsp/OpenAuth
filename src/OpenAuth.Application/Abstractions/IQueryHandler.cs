namespace OpenAuth.Application.Abstractions;

public interface IQueryHandler<in TQuery, TResult> : IHandler where TQuery : IQuery<TResult>
{
	Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}