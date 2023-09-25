using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace OAuthServer
{
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
                        "ForumAPI",
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email
                    },

                    RedirectUris = { "https://localhost:7185/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:7185/signout-callback-oidc" },

                    AlwaysIncludeUserClaimsInIdToken = true,
                    AlwaysSendClientClaims = true,

                    AccessTokenLifetime = 360000,

                    AllowOfflineAccess = true,
                },
            };

        public static IEnumerable<ApiResource> GetApiResources() =>
            new List<ApiResource>
            {
                new ApiResource("OrdersAPI")
            };

        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
    }
}
