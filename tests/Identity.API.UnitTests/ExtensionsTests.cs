using Duende.IdentityServer.Models;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class ExtensionsTests
{
    [TestMethod]
    public void IsNativeClient_false_for_https_redirect()
    {
        var request = new AuthorizationRequest { RedirectUri = "https://client.example/callback" };

        Assert.IsFalse(request.IsNativeClient());
    }

    [TestMethod]
    public void IsNativeClient_false_for_http_redirect()
    {
        var request = new AuthorizationRequest { RedirectUri = "http://client.example/callback" };

        Assert.IsFalse(request.IsNativeClient());
    }

    [TestMethod]
    public void IsNativeClient_true_for_custom_scheme_redirect()
    {
        var request = new AuthorizationRequest { RedirectUri = "myapp://callback" };

        Assert.IsTrue(request.IsNativeClient());
    }

    // The prefix check uses StringComparison.Ordinal (case-sensitive), so an
    // uppercase scheme genuinely isn't recognized as http/https here - a real
    // behavior of the actual code, not a test artifact.
    [TestMethod]
    public void IsNativeClient_true_for_uppercase_scheme_due_to_case_sensitive_comparison()
    {
        var request = new AuthorizationRequest { RedirectUri = "HTTPS://client.example/callback" };

        Assert.IsTrue(request.IsNativeClient());
    }
}
