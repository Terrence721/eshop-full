using System.Security.Claims;
using eShop.WebhookClient.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eShop.WebhookClient.UnitTests
{
    [TestClass]
    public class BffEndpointsTests
    {
        [TestMethod]
        public void SanitizeReturnUrl_defaults_to_the_app_root_when_empty_or_null()
        {
            Assert.AreEqual("/", BffEndpoints.SanitizeReturnUrl(null));
            Assert.AreEqual("/", BffEndpoints.SanitizeReturnUrl(string.Empty));
        }

        [TestMethod]
        public void SanitizeReturnUrl_passes_through_a_path_that_already_starts_with_a_slash()
        {
            Assert.AreEqual("/account/orders", BffEndpoints.SanitizeReturnUrl("/account/orders"));
        }

        [TestMethod]
        public void SanitizeReturnUrl_prefixes_a_relative_path_missing_its_leading_slash()
        {
            Assert.AreEqual("/account", BffEndpoints.SanitizeReturnUrl("account"));
        }

        [TestMethod]
        public void SanitizeReturnUrl_strips_the_host_from_an_absolute_url_to_prevent_open_redirects()
        {
            Assert.AreEqual("/phish", BffEndpoints.SanitizeReturnUrl("https://evil.example.com/phish"));
        }

        [TestMethod]
        public void Login_sets_the_sanitized_return_url_as_the_challenge_redirect()
        {
            var result = (ChallengeHttpResult)BffEndpoints.Login("account");

            Assert.AreEqual("/account", result.Properties!.RedirectUri);
        }

        [TestMethod]
        public void GetUser_returns_unauthorized_when_not_authenticated()
        {
            var result = BffEndpoints.GetUser(new ClaimsPrincipal(new ClaimsIdentity()));

            Assert.IsInstanceOfType<UnauthorizedHttpResult>(result);
        }

        [TestMethod]
        public void GetUser_returns_the_claims_when_authenticated()
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "Alice")], "test");
            var user = new ClaimsPrincipal(identity);

            // GetUser's result wraps an anonymous type (IEnumerable<{Type, Value}>), which can't
            // be named here - IValueHttpResult.Value (object?) plus IEnumerable<T>'s covariance
            // lets this get at the values without needing that type's name. Plain reflection,
            // not dynamic, since anonymous types are internal and the DLR binder can't resolve
            // members across the assembly boundary.
            var ok = (IValueHttpResult)BffEndpoints.GetUser(user);
            var claim = ((IEnumerable<object>)ok.Value!).Single();
            var claimType = claim.GetType();

            Assert.AreEqual(ClaimTypes.Name, claimType.GetProperty("Type")!.GetValue(claim));
            Assert.AreEqual("Alice", claimType.GetProperty("Value")!.GetValue(claim));
        }
    }
}
