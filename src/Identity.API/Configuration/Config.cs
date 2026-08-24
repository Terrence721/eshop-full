using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace eShop.Identity.API.Configuration;

public class Config
{
    // token lifetime shared by every client below that sets it
    private const int TokenLifetimeSeconds = 60 * 60 * 2; // 2 hours

    // ApiResources define the apis in your system
    public static IEnumerable<ApiResource> GetApis()
    {
        return new List<ApiResource>
        {
            new ApiResource("orders", "Orders Service"),
            new ApiResource("basket", "Basket Service"),
            new ApiResource("webhooks", "Webhooks registration Service"),
        };
    }

    // ApiScope is used to protect the API
    // The effect is the same as that of API resources in IdentityServer 3.x
    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new List<ApiScope>
        {
            new ApiScope("orders", "Orders Service"),
            new ApiScope("basket", "Basket Service"),
            new ApiScope("webhooks", "Webhooks registration Service"),
        };
    }

    // Identity resources are data like user ID, name, or email address of a user
    // see: http://docs.identityserver.io/en/release/configuration/resources.html
    public static IEnumerable<IdentityResource> GetResources()
    {
        return new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
        };
    }

    // client want to access resources (aka scopes)
    public static IEnumerable<Client> GetClients(IConfiguration configuration)
    {
        var mauiCallback = configuration.GetRequiredValue("MauiCallback");
        var webAppClient = configuration.GetRequiredValue("WebAppClient");
        var webhooksWebClient = configuration.GetRequiredValue("WebhooksWebClient");
        var basketApiClient = configuration.GetRequiredValue("BasketApiClient");
        var orderingApiClient = configuration.GetRequiredValue("OrderingApiClient");
        var webhooksApiClient = configuration.GetRequiredValue("WebhooksApiClient");

        return new List<Client>
        {
            new Client
            {
                ClientId = "maui",
                ClientName = "eShop MAUI OpenId Client",
                AllowedGrantTypes = GrantTypes.Code,
                //Used to retrieve the access token on the back channel.
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                RedirectUris = { mauiCallback },
                RequireConsent = false,
                RequirePkce = true,
                PostLogoutRedirectUris = { $"{mauiCallback}/Account/Redirecting" },
                //AllowedCorsOrigins = { "http://eshopxamarin" },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "orders",
                    "basket",
                    "mobileshoppingagg",
                    "webhooks"
                },
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
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha256())
                },
                ClientUri = webAppClient,                             // public uri of the client
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = false,
                RequireConsent = false,
                AllowOfflineAccess = true,
                AlwaysIncludeUserClaimsInIdToken = true,
                RequirePkce = false,
                RedirectUris = new List<string>
                {
                    $"{webAppClient}/signin-oidc"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    $"{webAppClient}/signout-callback-oidc"
                },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "orders",
                    "basket",
                    "webshoppingagg",
                    "webhooks"
                },
                AccessTokenLifetime = TokenLifetimeSeconds,
                IdentityTokenLifetime = TokenLifetimeSeconds
            },
            new Client
            {
                ClientId = "webhooksclient",
                ClientName = "Webhooks Client",
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha256())
                },
                ClientUri = webhooksWebClient,                             // public uri of the client
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = false,
                RequireConsent = false,
                AllowOfflineAccess = true,
                AlwaysIncludeUserClaimsInIdToken = true,
                RedirectUris = new List<string>
                {
                    $"{webhooksWebClient}/signin-oidc"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    $"{webhooksWebClient}/signout-callback-oidc"
                },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "webhooks"
                },
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
        };
    }
}
