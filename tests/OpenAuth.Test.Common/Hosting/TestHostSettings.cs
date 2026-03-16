using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpenAuth.Test.Common.Hosting;

public sealed record TestHostSettings(
	Action<DbContextOptionsBuilder>? DbConfigure,
	IReadOnlyList<Action<IServiceCollection>> ServiceOverrides,
	IReadOnlyList<Action<IConfigurationBuilder>> ConfigOverrides
);