using Microsoft.Extensions.DependencyInjection;

namespace OpenAuth.Test.Common.Hosting;

public class TestScope(IServiceScope scope) : IAsyncDisposable
{
	public T Resolve<T>() where T : notnull =>
		scope.ServiceProvider.GetRequiredService<T>();

	public ValueTask DisposeAsync()
	{
		scope.Dispose();
		return ValueTask.CompletedTask;
	}
}