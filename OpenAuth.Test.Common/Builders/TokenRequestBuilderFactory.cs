using OpenAuth.Domain.AuthorizationGrants;
using OpenAuth.Domain.Clients.ValueObjects;

namespace OpenAuth.Test.Common.Builders;

public static class TokenRequestBuilderFactory
{
    public static AuthorizationCodeTokenRequestBuilder BuildAuthorizationCodeRequest(ClientId clientId, AuthorizationGrant authGrant)
        => new(clientId, authGrant);

    public static ClientCredentialsTokenRequestBuilder BuildClientCredentialsRequest(ClientId clientId, string clientSecret)
        => new(clientId, clientSecret);
}