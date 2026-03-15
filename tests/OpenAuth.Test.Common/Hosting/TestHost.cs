using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Api;

namespace OpenAuth.Test.Common.Hosting;

public class TestHost(WebApplicationFactory<Program> factory)
{
	private IServiceProvider Services => factory.Services;

	public HttpClient CreateClient() =>
		factory.CreateClient(new WebApplicationFactoryClientOptions
			{ AllowAutoRedirect = false });

	public TModule CreateModuleAsync<TModule>(Func<IServiceScope, TModule> func)
	{
		var scope = Services.CreateAsyncScope();
		return func(scope);
	}
	
	public TestScope CreateScope() => new TestScope(Services.CreateScope());

	public async ValueTask DisposeAsync() => await factory.DisposeAsync();
}