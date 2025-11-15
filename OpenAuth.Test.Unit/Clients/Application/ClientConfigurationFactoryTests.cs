using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Services;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Application;

public class ClientConfigurationFactoryTests
{
    private readonly ClientConfigurationFactory _sut= new();
    

    [Fact]
    public void Create_WhenValidRequest_MapsBasicFieldsCorrectly()
    {
        var request = new RegisterClientRequest(
            "m2m",
            "client-name",
            new Dictionary<string, IEnumerable<string>>
            { { "api", ["read", "write"] } },
            ["https://example.com/callback"]
        );

        var config = _sut.Create(request);

        Assert.Equal(ClientName.Create("client-name"), config.Name);
        Assert.Equal(ClientApplicationTypes.M2M, config.ApplicationType);
        
        var audience = Assert.Single(config.AllowedAudiences);
        Assert.Equal("api", audience.Name.Value);
        Assert.Contains("read write", audience.Scopes.ToString());
        
        var uri = Assert.Single(config.RedirectUris);
        Assert.Equal("https://example.com/callback", uri.Value);
    }

    [Fact]
    public void Create_WhenPermissionsIsNull_ReturnsEmptyAudiences()
    {
        var request = new RegisterClientRequest("spa", "client", null, null);

        var config = _sut.Create(request);

        Assert.Empty(config.AllowedAudiences);
    }

    [Fact]
    public void Create_AssignsDefaultGrantTypes_FromApplicationType()
    {
        var request = new RegisterClientRequest("spa", "client", null, null);
        var config = _sut.Create(request);

        Assert.Equal(ClientApplicationTypes.Spa.DefaultGrantTypes, config.AllowedGrantTypes);
    } 
    
    [Fact]
    public void Create_WhenRequestIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(()
            => _sut.Create(null!));
    }
}