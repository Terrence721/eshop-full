using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace eShop.Identity.API.Configuration
{
    public class Config
    {
        // token lifetime shared by every client below that sets it
        private const int TokenLifetimeSeconds = 60 * 60 * 2; // 2 hours

        // ApiResources define the apis in your system
        public static IEnumerable<ApiResource> GetApis()
        {
            return
            [
                new ApiResource("orders", "Orders Service"),
                new ApiResource("basket", "Basket Service")
                {
                    Scopes = { "basket" }
                },
                new ApiResource("webhooks", "Webhooks registration Service"),
            ];
        }

        // ApiScope is used to protect the API
        // The effect is the same as that of API resources in IdentityServer 3.x
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return
            [
                new ApiScope("orders", "Orders Service"),
                new ApiScope("basket", "Basket Service"),
                new ApiScope("webhooks", "Webhooks registration Service"),
            ];
        }

        // Identity resources are data like user ID, name, or email address of a user
        // see: http://docs.identityserver.io/en/release/configuration/resources.html
        public static IEnumerable<IdentityResource> GetResources()
        {
            return
            [
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            ];
        }

        // client want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            var clientAppCallback = configuration.GetRequiredValue("ClientAppCallback");
            var webAppClient = configuration.GetRequiredValue("WebAppClient");
            var webBffClient = configuration.GetRequiredValue("WebBffClient");
            var webhooksWebClient = configuration.GetRequiredValue("WebhooksWebClient");
            var basketApiClient = configuration.GetRequiredValue("BasketApiClient");
            var orderingApiClient = configuration.GetRequiredValue("OrderingApiClient");
            var webhooksApiClient = configuration.GetRequiredValue("WebhooksApiClient");

            return
            [
                new Client
                {
                    ClientId = "eshop-client",
                    ClientName = "eShop Client OpenId Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    //Used to retrieve the access token on the back channel.
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    RedirectUris = { clientAppCallback },
                    RequireConsent = false,
                    RequirePkce = true,
                    PostLogoutRedirectUris = { $"{clientAppCallback}/Account/Redirecting" },
                    //AllowedCorsOrigins = { "http://eshopxamarin" },
                    AllowedScopes =
                    [
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket",
                        "mobileshoppingagg",
                        "webhooks"
                    ],
                    //Allow requesting refresh tokens for long lived API access
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AccessTokenLifetime = TokenLifetimeSeconds,
                    IdentityTokenLifetime = TokenLifetimeSeconds
                },
                new Client
                {
                    ClientId = "webapp",
                    ClientName = "WebApp Client",
                    ClientSecrets =
                    [
                        new Secret("secret".Sha256())
                    ],
                    ClientUri = webAppClient,                             // public uri of the client (the SPA itself)
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = true,
                    // WebBFF performs the actual OIDC code exchange -- the browser never leaves
                    // its origin during the redirect dance, so these must point at WebBFF's own
                    // /signin-oidc, not the SPA's. Confirmed via Duende.BFF's real architecture:
                    // the client's server-side session lives in WebBFF, not in the browser.
                    RedirectUris =
                    [
                        $"{webBffClient}/signin-oidc"
                    ],
                    PostLogoutRedirectUris =
                    [
                        $"{webBffClient}/signout-callback-oidc"
                    ],
                    AllowedScopes =
                    [
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "orders",
                        "basket"
                    ],
                    AccessTokenLifetime = TokenLifetimeSeconds,
                    IdentityTokenLifetime = TokenLifetimeSeconds
                },
                new Client
                {
                    ClientId = "webhooksclient",
                    ClientName = "Webhooks Client",
                    ClientSecrets =
                    [
                        new Secret("secret".Sha256())
                    ],
                    ClientUri = webhooksWebClient,                             // public uri of the client
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowAccessTokensViaBrowser = false,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris =
                    [
                        $"{webhooksWebClient}/signin-oidc"
                    ],
                    PostLogoutRedirectUris =
                    [
                        $"{webhooksWebClient}/signout-callback-oidc"
                    ],
                    AllowedScopes =
                    [
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "webhooks"
                    ],
                    AccessTokenLifetime = TokenLifetimeSeconds,
                    IdentityTokenLifetime = TokenLifetimeSeconds
                },
                new Client
                {
                    ClientId = "basketswaggerui",
                    ClientName = "Basket Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{basketApiClient}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{basketApiClient}/swagger/" },

                    AllowedScopes =
                    {
                        "basket"
                    }
                },
                new Client
                {
                    ClientId = "orderingswaggerui",
                    ClientName = "Ordering Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{orderingApiClient}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{orderingApiClient}/swagger/" },

                    AllowedScopes =
                    {
                        "orders"
                    }
                },
                new Client
                {
                    ClientId = "webhooksswaggerui",
                    ClientName = "WebHooks Service Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { $"{webhooksApiClient}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{webhooksApiClient}/swagger/" },

                    AllowedScopes =
                    {
                        "webhooks"
                    }
                }
            ];
        }
    }
}
