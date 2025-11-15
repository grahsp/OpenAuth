using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using OpenAuth.Domain.Clients.ApplicationType;
using OpenAuth.Domain.Clients.Factories;
using OpenAuth.Domain.Clients.Secrets.ValueObjects;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.Services;
using OpenAuth.Domain.Services.Dtos;

namespace OpenAuth.Test.Unit.Clients.Domain;

public class ClientFactoryTests
{
    private readonly ISecretHashProvider _hashProvider;
    private readonly FakeTimeProvider _time;
    private readonly IClientFactory _sut;
    
    public ClientFactoryTests()
    {
        _hashProvider = Substitute.For<ISecretHashProvider>();
        _time = new FakeTimeProvider();

        _sut = new ClientFactory(_hashProvider, _time);
    }
    


    [Fact]
    public void Create_WhenPublicClient_DoesNotGenerateSecret()
    {
        var config = new ClientConfiguration(
            ClientName.Create("client"),
            ClientApplicationTypes.Spa,
            [],
            [],
            [RedirectUri.Create("https://example.com/callback")]
        );

        var client = _sut.Create(config, out var secret);

        Assert.Null(secret);
        _hashProvider.DidNotReceive().Create();
        Assert.Empty(client.Secrets);
    }

    [Fact]
    public void Create_WhenConfidentialClient_GeneratesSecret_AndAddsToClient()
    {
        var config = new ClientConfiguration(
            ClientName.Create("client"),
            ClientApplicationTypes.M2M,
            [new Audience(AudienceName.Create("api"), ScopeCollection.Parse("read write"))],
            [],
            []
        );

        var hashResult = new SecretCreationResult("plain-secret", SecretHash.FromHash("hashed-value"));
        _hashProvider.Create().Returns(hashResult);

        var client = _sut.Create(config, out var plainSecret);

        Assert.Equal("plain-secret", plainSecret);
        Assert.Contains(client.Secrets, s => s.Hash == hashResult.Hash);
        _hashProvider.Received(1).Create();
    }

    [Fact]
    public void Create_CallsValidateClient()
    {
        var applicationType = ClientApplicationTypes.Spa;
        var invalidConfig = new ClientConfiguration(
            ClientName.Create("client"),
            applicationType,
            [],
            applicationType.DefaultGrantTypes,
            []
        );

        // Throws due to missing redirect required by AuthorizationCode flow
        Assert.Throws<InvalidOperationException>(() =>
            _sut.Create(invalidConfig, out _));
    }

    [Fact]
    public void Create_UsesCurrentTime_FromTimeProvider()
    {
        var expected = _time.GetUtcNow();

        var config = new ClientConfiguration(
            ClientName.Create("client"),
            ClientApplicationTypes.Spa,
            [],
            [],
            [RedirectUri.Create("https://example.com/callback")]
        );

        var hashResult = new SecretCreationResult("plain", SecretHash.FromHash("hash"));
        _hashProvider.Create().Returns(hashResult);

        var client = _sut.Create(config, out _);

        Assert.All(client.Secrets, s => Assert.Equal(expected, s.CreatedAt));
    }
    
    [Fact]
    public void Create_WhenConfigIsNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Create(null!, out _));
    }
}