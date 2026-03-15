using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OpenAuth.Api;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Common.Hosting;

public class TestHost(WebApplicationFactory<Program> factory)
{
	private IServiceProvider Services => factory.Services;

	public HttpClient CreateClient()
	{
		var client = factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false,
		});
		
		client.DefaultRequestHeaders.Add("X-Test-User", DefaultValues.UserId);
		return client;
	}

	public TModule CreateModuleAsync<TModule>(Func<IServiceScope, TModule> func)
	{
		var scope = Services.CreateAsyncScope();
		return func(scope);
	}
	
	public TestScope CreateScope() => new TestScope(Services.CreateScope());

	public async ValueTask DisposeAsync() => await factory.DisposeAsync();
}