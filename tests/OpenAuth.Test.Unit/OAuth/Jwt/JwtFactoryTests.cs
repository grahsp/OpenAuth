using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using OpenAuth.Application.OAuth.Jwts;
using OpenAuth.Application.Security.Signing;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.OAuth;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Infrastructure.OAuth.Jwt;

namespace OpenAuth.Test.Unit.OAuth.Jwt;

public class JwtFactoryTests
{
    private const string SecretKey = "this-is-a-test-secret-key-that-is-very-secret!";
    
    private readonly ISigningKeyQueryService _keyService;
    private readonly ISigningCredentialsFactory _credentialsFactory;

    private readonly FakeTimeProvider _time;
    private readonly IJwtFactory _sut;

    public JwtFactoryTests()
    {
        _keyService = Substitute.For<ISigningKeyQueryService>();
        _credentialsFactory = Substitute.For<ISigningCredentialsFactory>();

        var keyData = new SigningKeyData(SigningKeyId.New(), KeyType.HMAC, SigningAlgorithm.HS256, new Key(SecretKey));
        _keyService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns(keyData);

        var signingCredentials = CreateSigningCredentials(keyData);
        _credentialsFactory.Create(keyData)
            .Returns(signingCredentials);
        
        _time = new FakeTimeProvider();
        _sut = new JwtFactory(_keyService, _credentialsFactory);
    }

    private SigningCredentials CreateSigningCredentials(SigningKeyData keyData)
    {
        var bytes= Encoding.UTF8.GetBytes(keyData.Key.Value);
        var securityKey = new SymmetricSecurityKey(bytes) { KeyId = keyData.Kid.Value.ToString() };
        
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    private JwtBuilder CreateValidBuilder()
    {
        return new JwtBuilder("issuer")
            .WithClient(ClientId.New())
            .WithSubject("subject")
            .WithAudience(new AudienceName("api"));
    }

    private JwtDescriptor CreateValidDescriptor()
    {
        return CreateValidBuilder()
            .Build(_time);
    }


    [Fact]
    public async Task Create_SetsCorrectMetadata()
    {
        var lifetime = TimeSpan.FromMinutes(5);
        var descriptor = CreateValidBuilder()
            .WithLifetime(lifetime)
            .Build(_time);

        var result = await _sut.Create(descriptor);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(TokenType.Bearer, result.TokenType);
        Assert.Equal((int)lifetime.TotalSeconds, result.ExpiresIn);
    }

    [Fact]
    public async Task Create_IncludesClaimsFromDescriptor()
    {
        var descriptor = CreateValidDescriptor();

        var result = await _sut.Create(descriptor);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        
        Assert.NotEmpty(jwt.Claims);
        Assert.Equal(descriptor.Claims.Count, jwt.Claims.Count());
    }

    [Fact]
    public async Task Create_WithMultipleScopes_IncludesAllScopes()
    {
        var descriptor = CreateValidBuilder()
            .WithScopes(new Scope("read"), new Scope("write"))
            .Build(_time);
        
        var result = await _sut.Create(descriptor);
        
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);
        
        var scopes = jwt.Claims.Where(c => c.Type == OAuthClaimTypes.Scope).ToArray();
        Assert.Contains(scopes, c => c.Value == "read");
        Assert.Contains(scopes, c => c.Value == "write");
    }

    [Fact]
    public async Task Create_CallsKeyService()
    {
        var descriptor = CreateValidDescriptor();
        
        await _sut.Create(descriptor);
        
        await _keyService.Received(1).GetCurrentKeyDataAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_PassesCancellationToken()
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var descriptor = CreateValidDescriptor();
        
        await _sut.Create(descriptor, token);
        
        await _keyService.Received(1).GetCurrentKeyDataAsync(token);
    }

    [Fact]
    public async Task Create_WhenNoActiveKey_ThrowsException()
    {
        var descriptor = CreateValidDescriptor();
        
        _keyService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
            .Returns((SigningKeyData)null!);
        
        var result = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Create(descriptor));
        
        Assert.Contains("signing key", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_CallsSigningCredentialsFactory()
    {
        var descriptor = CreateValidDescriptor();

        await _sut.Create(descriptor);
        
        _credentialsFactory.Received(1).Create(Arg.Any<SigningKeyData>());
    }
}