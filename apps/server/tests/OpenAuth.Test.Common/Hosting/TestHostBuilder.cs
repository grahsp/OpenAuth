using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenAuth.Test.Common.Hosting;

public sealed class TestHostBuilder
{
	private readonly List<Action<IServiceCollection>> _servicesOverrides = [];
	private readonly List<Action<IConfigurationBuilder>> _configurationOverrides = [];

	private Action<DbContextOptionsBuilder>? _dbConfigure;

	public TestHostBuilder UseDatabase(Action<DbContextOptionsBuilder> configure)
	{
		_dbConfigure = configure;
		return this;
	}
	
	public TestHostBuilder ConfigureServices(Action<IServiceCollection> configure)
	{
		_servicesOverrides.Add(configure);
		return this;
	}

	public TestHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configure)
	{
		_configurationOverrides.Add(configure);
		return this;
	}

	public TestHostSettings Build()
	{
		return new TestHostSettings(_dbConfigure, _servicesOverrides, _configurationOverrides);
	}
}