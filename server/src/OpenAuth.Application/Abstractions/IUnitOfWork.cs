namespace OpenAuth.Application.Abstractions;

public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken ct);
}