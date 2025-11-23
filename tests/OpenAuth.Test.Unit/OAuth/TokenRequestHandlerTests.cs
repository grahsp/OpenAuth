using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OpenAuth.Application.Clients.Dtos;
using OpenAuth.Application.Clients.Interfaces;
using OpenAuth.Application.Exceptions;
using OpenAuth.Application.SigningKeys.Dtos;
using OpenAuth.Application.SigningKeys.Interfaces;
using OpenAuth.Application.Tokens.Dtos;
using OpenAuth.Application.Tokens.Flows;
using OpenAuth.Application.Tokens.Interfaces;
using OpenAuth.Application.Tokens.Services;
using OpenAuth.Domain.Clients.ValueObjects;
using OpenAuth.Domain.SigningKeys.Enums;
using OpenAuth.Domain.SigningKeys.ValueObjects;
using OpenAuth.Test.Common.Builders;
using OpenAuth.Test.Common.Helpers;

namespace OpenAuth.Test.Unit.OAuth;

// public class TokenRequestHandlerTests
// {
//     private readonly IClientQueryService _clientQueryService;
//     private readonly ISigningKeyQueryService _signingKeyQueryService;
//     private readonly IJwtTokenGenerator _tokenGenerator;
//     private readonly ITokenRequestProcessor _clientCredentialsRequestProcessor;
//     private readonly ITokenRequestProcessor _authorizationCodeRequestProcessor;
//     private readonly TokenRequestHandler _tokenRequestHandler;
//
//     public TokenRequestHandlerTests()
//     {
//         _clientQueryService = Substitute.For<IClientQueryService>();
//         _signingKeyQueryService = Substitute.For<ISigningKeyQueryService>();
//         _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
//         _clientCredentialsRequestProcessor = Substitute.For<ITokenRequestProcessor>();
//         _authorizationCodeRequestProcessor = Substitute.For<ITokenRequestProcessor>();
//
//         _clientCredentialsRequestProcessor.GrantType.Returns(GrantType.ClientCredentials);
//         _authorizationCodeRequestProcessor.GrantType.Returns(GrantType.AuthorizationCode);
//
//         _tokenRequestHandler = new TokenRequestHandler(
//             [_clientCredentialsRequestProcessor, _authorizationCodeRequestProcessor],
//             _clientQueryService,
//             _signingKeyQueryService,
//             _tokenGenerator);
//     }
//
//
//     [Fact]
//     public void Constructor_WithDuplicateGrantTypes_ThrowsArgumentException()
//     {
//         var issuer1 = Substitute.For<ITokenRequestProcessor>();
//         var issuer2 = Substitute.For<ITokenRequestProcessor>();
//         issuer1.GrantType.Returns(GrantType.ClientCredentials);
//         issuer2.GrantType.Returns(GrantType.ClientCredentials);
//
//         Assert.Throws<ArgumentException>(() =>
//             new TokenRequestHandler(
//                 [issuer1, issuer2],
//                 _clientQueryService,
//                 _signingKeyQueryService,
//                 _tokenGenerator
//             ));
//     }
//
//     [Fact]
//     public void Constructor_WithNoIssuers_ThrowsArgumentException()
//     {
//         Assert.Throws<ArgumentException>(() =>
//             new TokenRequestHandler(
//                 [],
//                 _clientQueryService,
//                 _signingKeyQueryService,
//                 _tokenGenerator
//             ));
//     }
//
//     [Fact]
//     public async Task IssueToken_WithUnsupportedGrantType_ThrowsInvalidRequestException()
//     {
//         var tokenService = new TokenRequestHandler(
//             [_authorizationCodeRequestProcessor],
//             _clientQueryService,
//             _signingKeyQueryService,
//             _tokenGenerator);
//
//         var request = CreateValidRequest();
//
//         await Assert.ThrowsAsync<InvalidRequestException>(
//             () => tokenService.IssueToken(request));
//     }
//
//     [Fact]
//     public async Task IssueToken_WithMismatchedGrantType_ThrowsInvalidRequestException()
//     {
//         var otherIssuer = Substitute.For<ITokenRequestProcessor>();
//         otherIssuer.GrantType.Returns(GrantType.AuthorizationCode);
//
//         var tokenService = new TokenRequestHandler(
//             [otherIssuer],
//             _clientQueryService,
//             _signingKeyQueryService,
//             _tokenGenerator);
//
//         await Assert.ThrowsAsync<InvalidRequestException>(
//             () => tokenService.IssueToken(CreateValidRequest()));
//     }
//
//     [Fact]
//     public async Task IssueToken_WithClientNotFound_ThrowsInvalidClientException()
//     {
//         var request = CreateValidRequest();
//
//         _clientQueryService
//             .GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
//             .Returns((ClientTokenData?)null);
//
//         await Assert.ThrowsAsync<InvalidClientException>(
//             () => _tokenRequestHandler.IssueToken(request));
//     }
//
//     [Fact]
//     public async Task IssueToken_WithInvalidScopes_ThrowsInvalidScopeException()
//     {
//         var request = new AuthorizationCodeTokenCommandBuilder()
//             .WithScopes("invalid_scopes")
//             .WithCodeVerifier(DefaultValues.CodeVerifier)
//             .Build();
//         var tokenData = CreateValidTokenData();
//
//         _clientQueryService
//             .GetTokenDataAsync(request.ClientId, Arg.Any<CancellationToken>())
//             .Returns(tokenData);
//
//         await Assert.ThrowsAsync<InvalidScopeException>(
//             () => _tokenRequestHandler.IssueToken(request));
//     }
//
//     [Fact]
//     public async Task IssueToken_WithNoActiveSigningKey_ThrowsServerErrorException()
//     {
//         var request = CreateValidRequest();
//         var tokenData = CreateValidTokenData();
//         var tokenContext = CreateValidTokenContext();
//
//         _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
//             .Returns(tokenData);
//         _clientCredentialsRequestProcessor.ProcessAsync(Arg.Any<TokenCommand>(), Arg.Any<CancellationToken>())
//             .Returns(tokenContext);
//         _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
//             .Returns((SigningKeyData?)null);
//
//         await Assert.ThrowsAsync<ServerErrorException>(
//             () => _tokenRequestHandler.IssueToken(request));
//     }
//
//     [Fact]
//     public async Task IssueToken_WhenTokenGeneratorThrows_PropagatesException()
//     {
//         var request = CreateValidRequest();
//         var tokenData = CreateValidTokenData();
//         var tokenContext = CreateValidTokenContext();
//         var keyData = CreateValidKeyData();
//
//         _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
//             .Returns(tokenData);
//         _clientCredentialsRequestProcessor.ProcessAsync(Arg.Any<TokenCommand>(), Arg.Any<CancellationToken>())
//             .Returns(tokenContext);
//         _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
//             .Returns(keyData);
//         _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData)
//             .Throws(new InvalidOperationException("Signing failed"));
//
//         await Assert.ThrowsAsync<InvalidOperationException>(
//             () => _tokenRequestHandler.IssueToken(request));
//     }
//
//     [Fact]
//     public async Task IssueToken_WithValidRequest_ReturnsTokenResponse()
//     {
//         var request = CreateValidRequest();
//         var tokenData = CreateValidTokenData();
//         var tokenContext = CreateValidTokenContext();
//         var keyData = CreateValidKeyData();
//         const string generatedToken = "token-value";
//
//         _clientQueryService.GetTokenDataAsync(request.ClientId, Arg.Any<CancellationToken>())
//             .Returns(tokenData);
//         _clientCredentialsRequestProcessor.ProcessAsync(request, Arg.Any<CancellationToken>())
//             .Returns(tokenContext);
//         _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
//             .Returns(keyData);
//         _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData)
//             .Returns(generatedToken);
//
//         var result = await _tokenRequestHandler.IssueToken(request);
//
//         Assert.NotNull(result);
//         Assert.Equal(generatedToken, result.Token);
//         Assert.Equal("Bearer", result.TokenType);
//         Assert.Equal(3600, result.ExpiresIn);
//     }
//
//     [Fact]
//     public async Task IssueToken_PassesCancellationTokenThroughChain()
//     {
//         var cts = new CancellationTokenSource();
//         var token = cts.Token;
//         var request = CreateValidRequest();
//         var tokenData = CreateValidTokenData();
//         var tokenContext = CreateValidTokenContext();
//         var keyData = CreateValidKeyData();
//
//         _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), token)
//             .Returns(tokenData);
//         _clientCredentialsRequestProcessor.ProcessAsync(Arg.Any<TokenCommand>(), token)
//             .Returns(tokenContext);
//         _signingKeyQueryService.GetCurrentKeyDataAsync(token)
//             .Returns(keyData);
//         _tokenGenerator.GenerateToken(Arg.Any<TokenContext>(), Arg.Any<ClientTokenData>(), Arg.Any<SigningKeyData>())
//             .Returns("token");
//
//         await _tokenRequestHandler.IssueToken(request, token);
//
//         await _clientQueryService.Received(1).GetTokenDataAsync(request.ClientId, token);
//         await _clientCredentialsRequestProcessor.Received(1).ProcessAsync(request, token);
//         await _signingKeyQueryService.Received(1).GetCurrentKeyDataAsync(token);
//     }
//
//     [Fact]
//     public async Task IssueToken_WhenIssuerThrows_PropagatesException()
//     {
//         var request = CreateValidRequest();
//         var tokenData = CreateValidTokenData();
//
//         _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
//             .Returns(tokenData);
//         _clientCredentialsRequestProcessor.ProcessAsync(Arg.Any<TokenCommand>(), Arg.Any<CancellationToken>())
//             .Throws(new UnauthorizedAccessException("Invalid credentials"));
//
//         await Assert.ThrowsAsync<UnauthorizedAccessException>(
//             () => _tokenRequestHandler.IssueToken(request));
//     }
//
//     [Fact]
//     public async Task IssueToken_CallsTokenGeneratorWithCorrectParameters()
//     {
//         var request = CreateValidRequest();
//         var tokenData = CreateValidTokenData();
//         var tokenContext = CreateValidTokenContext();
//         var keyData = CreateValidKeyData();
//
//         _clientQueryService.GetTokenDataAsync(Arg.Any<ClientId>(), Arg.Any<CancellationToken>())
//             .Returns(tokenData);
//         _clientCredentialsRequestProcessor.ProcessAsync(Arg.Any<TokenCommand>(), Arg.Any<CancellationToken>())
//             .Returns(tokenContext);
//         _signingKeyQueryService.GetCurrentKeyDataAsync(Arg.Any<CancellationToken>())
//             .Returns(keyData);
//         _tokenGenerator.GenerateToken(tokenContext, tokenData, keyData)
//             .Returns("token");
//
//         await _tokenRequestHandler.IssueToken(request);
//
//         _tokenGenerator.Received(1).GenerateToken(tokenContext, tokenData, keyData);
//     }
//
//     
//     private static ClientCredentialsTokenCommand CreateValidRequest() => ClientCredentialsTokenCommand.Create(
//         ClientId.New(),
//         new AudienceName(DefaultValues.Audience),
//         ScopeCollection.Parse(DefaultValues.Scopes),
//         DefaultValues.ClientSecret
//     );
//
//     private static ClientTokenData CreateValidTokenData() => new(
//         AllowedAudiences: [new Audience(AudienceName.Create(DefaultValues.Audience), ScopeCollection.Parse(DefaultValues.Scopes))],
//         [GrantType.ClientCredentials, GrantType.AuthorizationCode],
//         TokenLifetime: TimeSpan.FromHours(1));
//
//     private static TokenContext CreateValidTokenContext()
//     {
//         var clientId = ClientId.New();
//         return new TokenContext(
//             clientId,
//             clientId.ToString(),
//             new AudienceName(DefaultValues.Audience),
//             ScopeCollection.Parse(DefaultValues.Scopes)
//         );
//     }
//     
//     private static SigningKeyData CreateValidKeyData()
//     {
//         return new SigningKeyData(
//             SigningKeyId.New(),
//             KeyType.RSA,
//             SigningAlgorithm.RS256,
//             new Key("key")
//         );
//     }
// }