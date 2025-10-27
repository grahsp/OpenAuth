using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenAuth.Api;
using OpenAuth.Infrastructure.Persistence;

namespace OpenAuth.Test.Integration.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContext>();
            services.AddDbContext<AppDbContext>(options =>
                SqlServerDbContextOptionsExtensions.UseSqlServer(options, _connectionString));
        });

        builder.UseEnvironment("Testing");
    }
}