using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace OAuthServer;

public static class Configuration
{
    public static IEnumerable<Client> GetClient() =>
        new List<Client>
        {
            new Client
            {
                ClientId = "test_client_id",
                ClientSecrets = {new Secret("test_client_secret".ToSha256())},

                AllowedGrantTypes = GrantTypes.Code,

                AllowedCorsOrigins = {"https://localhost:7185"},
                AllowedScopes =
                {
                    "ClientAPI",
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                },

                RedirectUris = { "https://localhost:7185/signin-oidc" },
                PostLogoutRedirectUris = { "http://localhost:7185/signout-callback-oidc" },

                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,

                AccessTokenLifetime = 360000,

                AllowOfflineAccess = true,
            },
            new Client
            {
                ClientId = "interactive.public",
                ClientName = "Interactive client (Code with PKCE)",

                RedirectUris = { "https://notused" },
                PostLogoutRedirectUris = { "https://notused" },

                RequireClientSecret = false,

                AllowedGrantTypes = GrantTypes.Code,
                AllowedScopes =
                {
                    "ClientAPI",
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                },

                AllowOfflineAccess = true,
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding
            },
        };

    public static IEnumerable<ApiResource> GetApiResources() =>
        new List<ApiResource>
        {
            new ApiResource("ClientAPI")
        };

    public static IEnumerable<IdentityResource> GetIdentityResources() =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };
}