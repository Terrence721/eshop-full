using eShop.Identity.API.Configuration;
using Microsoft.Extensions.Configuration;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class ConfigTests
{
    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] overrides)
    {
        var values = new Dictionary<string, string?>
        {
            ["MauiCallback"] = "maui://authcallback",
            ["WebAppClient"] = "https://webapp.example",
            ["WebhooksWebClient"] = "https://webhooksclient.example",
            ["BasketApiClient"] = "https://basket-api.example",
            ["OrderingApiClient"] = "https://ordering-api.example",
            ["WebhooksApiClient"] = "https://webhooks-api.example"
        };

        foreach (var (key, value) in overrides)
        {
            values[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [TestMethod]
    public void GetClients_returns_all_six_clients()
    {
        var clients = Config.GetClients(BuildConfiguration()).ToList();

        Assert.HasCount(6, clients);
        CollectionAssert.AreEquivalent(
            new[] { "maui", "webapp", "webhooksclient", "basketswaggerui", "orderingswaggerui", "webhooksswaggerui" },
            clients.Select(c => c.ClientId).Distinct().ToList());
    }

    [TestMethod]
    public void GetClients_substitutes_MauiCallback_into_the_maui_client_redirect_uris()
    {
        var clients = Config.GetClients(BuildConfiguration(("MauiCallback", "myapp://custom-callback")));

        var maui = clients.Single(c => c.ClientId == "maui");
        Assert.Contains("myapp://custom-callback", maui.RedirectUris);
        Assert.Contains("myapp://custom-callback/Account/Redirecting", maui.PostLogoutRedirectUris);
    }

    [TestMethod]
    public void GetClients_substitutes_WebAppClient_into_the_webapp_client_redirect_uris()
    {
        var clients = Config.GetClients(BuildConfiguration(("WebAppClient", "https://custom-webapp.example")));

        var webapp = clients.Single(c => c.ClientId == "webapp");
        Assert.AreEqual("https://custom-webapp.example", webapp.ClientUri);
        Assert.Contains("https://custom-webapp.example/signin-oidc", webapp.RedirectUris);
    }

    [TestMethod]
    public void GetClients_throws_when_a_required_config_value_is_missing()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.ThrowsExactly<InvalidOperationException>(() => Config.GetClients(configuration).ToList());
    }
}
