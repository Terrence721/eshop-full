using System.Buffers.Text;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class DiagnosticsControllerTests
{
    private static DiagnosticsController CreateController(
        string? remoteIp, string? localIp, AuthenticateResult authenticateResult)
    {
        var authService = Substitute.For<IAuthenticationService>();
        authService.AuthenticateAsync(Arg.Any<HttpContext>(), Arg.Any<string?>()).Returns(authenticateResult);

        var services = new ServiceCollection();
        services.AddSingleton(authService);

        var httpContext = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        if (remoteIp != null) httpContext.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
        if (localIp != null) httpContext.Connection.LocalIpAddress = IPAddress.Parse(localIp);

        return new DiagnosticsController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [TestMethod]
    public async Task Index_returns_NotFound_when_remote_address_is_not_loopback_or_local()
    {
        var controller = CreateController("203.0.113.5", "203.0.113.9", AuthenticateResult.NoResult());

        var result = await controller.Index();

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Index_returns_NotFound_when_remote_address_is_missing()
    {
        var controller = CreateController(null, null, AuthenticateResult.NoResult());

        var result = await controller.Index();

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Index_allows_ipv4_loopback()
    {
        var controller = CreateController("127.0.0.1", "203.0.113.9", AuthenticateResult.NoResult());

        var result = await controller.Index();

        Assert.IsInstanceOfType<OkObjectResult>(result.Result);
    }

    [TestMethod]
    public async Task Index_allows_ipv6_loopback()
    {
        var controller = CreateController("::1", "203.0.113.9", AuthenticateResult.NoResult());

        var result = await controller.Index();

        Assert.IsInstanceOfType<OkObjectResult>(result.Result);
    }

    [TestMethod]
    public async Task Index_allows_remote_address_matching_local_address()
    {
        var controller = CreateController("203.0.113.9", "203.0.113.9", AuthenticateResult.NoResult());

        var result = await controller.Index();

        Assert.IsInstanceOfType<OkObjectResult>(result.Result);
    }

    [TestMethod]
    public async Task Index_maps_the_authenticated_principals_claims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "user-1")]));
        var ticket = new AuthenticationTicket(principal, "scheme");
        var controller = CreateController("127.0.0.1", "127.0.0.1", AuthenticateResult.Success(ticket));

        var result = await controller.Index();

        var vm = ((OkObjectResult)result.Result!).Value as DiagnosticsViewModel;
        Assert.IsTrue(vm!.Claims.Any(c => c.Type == "sub" && c.Value == "user-1"));
    }

    [TestMethod]
    public async Task Index_decodes_client_list_when_present()
    {
        var json = JsonSerializer.Serialize(new[] { "client-a", "client-b" });
        var encoded = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(json));
        var properties = new AuthenticationProperties(new Dictionary<string, string?> { ["client_list"] = encoded });
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var ticket = new AuthenticationTicket(principal, properties, "scheme");
        var controller = CreateController("127.0.0.1", "127.0.0.1", AuthenticateResult.Success(ticket));

        var result = await controller.Index();

        var vm = ((OkObjectResult)result.Result!).Value as DiagnosticsViewModel;
        CollectionAssert.AreEqual(new[] { "client-a", "client-b" }, vm!.Clients.ToList());
    }

    [TestMethod]
    public async Task Index_leaves_Clients_empty_when_client_list_absent()
    {
        var controller = CreateController("127.0.0.1", "127.0.0.1", AuthenticateResult.NoResult());

        var result = await controller.Index();

        var vm = ((OkObjectResult)result.Result!).Value as DiagnosticsViewModel;
        Assert.IsEmpty(vm!.Clients);
    }
}
