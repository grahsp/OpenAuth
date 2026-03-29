using System.Net.Http.Json;
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
        
        var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
        var config = await http.GetFromJsonAsync<Config>("config.json");
        
        if (config is null)
            throw new Exception("Could not load config.json");
        
        builder.Services.AddOidcAuthentication(options =>
        {
            options.ProviderOptions.Authority = config.Authority;
            options.ProviderOptions.ClientId = "7b3f1d57-2068-4b9c-b86a-ac6d99838677";
            
            options.ProviderOptions.ResponseType = "code";
            options.ProviderOptions.RedirectUri = "authentication/login-callback";
            options.ProviderOptions.MetadataUrl = $"{config.Authority}/.well-known/openid-configuration";

            options.ProviderOptions.AdditionalProviderParameters.Add("audience", "http://weather-demo.com");
        });

        await builder.Build().RunAsync();
    }
}