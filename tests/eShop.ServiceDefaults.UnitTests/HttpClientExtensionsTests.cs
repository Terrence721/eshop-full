using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace eShop.ServiceDefaults.UnitTests;

[TestClass]
public class HttpClientExtensionsTests
{
    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private static async Task<HttpRequestMessage> SendAndCaptureRequestAsync(HttpContext? httpContext)
    {
        HttpRequestMessage? capturedRequest = null;

        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext });
        services.AddHttpClient("test")
            .AddAuthToken()
            .ConfigurePrimaryHttpMessageHandler(() => new StubHttpMessageHandler(request =>
            {
                capturedRequest = request;
                return new HttpResponseMessage(HttpStatusCode.OK);
            }));

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("test");

        await client.GetAsync("https://example.test/");

        return capturedRequest!;
    }

    private static HttpContext HttpContextWithToken(string? token)
    {
        var properties = new AuthenticationProperties();
        if (token is not null)
        {
            properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = token }]);
        }

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(), properties, "TestScheme");

        var authenticationService = Substitute.For<IAuthenticationService>();
        authenticationService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>())
            .Returns(AuthenticateResult.Success(ticket));

        var services = new ServiceCollection();
        services.AddSingleton(authenticationService);
        var provider = services.BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = provider };
    }

    [TestMethod]
    public async Task Request_has_no_authorization_header_when_HttpContext_is_null()
    {
        var request = await SendAndCaptureRequestAsync(httpContext: null);

        Assert.IsNull(request.Headers.Authorization);
    }

    [TestMethod]
    public async Task Request_has_no_authorization_header_when_access_token_missing()
    {
        var request = await SendAndCaptureRequestAsync(HttpContextWithToken(token: null));

        Assert.IsNull(request.Headers.Authorization);
    }

    [TestMethod]
    public async Task Request_has_bearer_authorization_header_when_access_token_present()
    {
        var request = await SendAndCaptureRequestAsync(HttpContextWithToken("test-token"));

        Assert.AreEqual("Bearer", request.Headers.Authorization?.Scheme);
        Assert.AreEqual("test-token", request.Headers.Authorization?.Parameter);
    }
}
