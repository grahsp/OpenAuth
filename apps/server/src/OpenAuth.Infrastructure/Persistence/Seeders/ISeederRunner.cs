namespace OpenAuth.Infrastructure.Persistence.Seeders;

public interface ISeederRunner
{
	Task RunAsync(CancellationToken ct);
}