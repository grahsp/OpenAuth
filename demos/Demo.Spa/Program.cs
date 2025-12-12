using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Demo.Spa;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddHttpClient("WeatherApi", client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            })
            .AddHttpMessageHandler(sp =>
            {
                var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
                    .ConfigureHandler(
                        authorizedUrls: ["http://localhost:5278"],
                        scopes: ["weather:read"]
                    );
                
                return handler;
            });
        
        builder.Services.AddOidcAuthentication(options =>
        {
            options.ProviderOptions.Authority = "http://localhost:5067";
            options.ProviderOptions.ClientId = "7b3f1d57-2068-4b9c-b86a-ac6d99838677";
            
            options.ProviderOptions.ResponseType = "code";
            options.ProviderOptions.RedirectUri = "authentication/login-callback";
            options.ProviderOptions.MetadataUrl = "http://localhost:5067/.well-known/openid-configuration";

            options.ProviderOptions.DefaultScopes.Add("email");
            options.ProviderOptions.DefaultScopes.Add("weather:read");
        });

        await builder.Build().RunAsync();
    }
}