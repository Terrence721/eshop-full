using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IdentityServerHost.Quickstart.UI.UnitTests;

[TestClass]
public class HomeControllerTests
{
    private static IWebHostEnvironment EnvironmentNamed(string name)
    {
        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns(name);
        return env;
    }

    [TestMethod]
    public void Index_returns_ok_with_static_urls_in_development()
    {
        var controller = new HomeController(
            Substitute.For<IIdentityServerInteractionService>(),
            EnvironmentNamed("Development"),
            Substitute.For<ILogger<HomeController>>());

        var result = controller.Index();

        var vm = ((OkObjectResult)result.Result!).Value as IndexViewModel;
        Assert.IsNotNull(vm);
        Assert.IsFalse(string.IsNullOrEmpty(vm.Version));
        Assert.AreEqual("/.well-known/openid-configuration", vm.WellKnownConfigurationUrl);
        Assert.AreEqual("/diagnostics", vm.DiagnosticsUrl);
        Assert.AreEqual("/grants", vm.GrantsUrl);
    }

    [TestMethod]
    public void Index_returns_NotFound_outside_development()
    {
        var controller = new HomeController(
            Substitute.For<IIdentityServerInteractionService>(),
            EnvironmentNamed("Production"),
            Substitute.For<ILogger<HomeController>>());

        var result = controller.Index();

        Assert.IsInstanceOfType<NotFoundResult>(result.Result);
    }

    [TestMethod]
    public async Task Error_leaves_ErrorDescription_intact_in_development()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetErrorContextAsync("error-1", Arg.Any<CancellationToken>())
            .Returns(new ErrorMessage { Error = "invalid_request", ErrorDescription = "a real description" });
        var controller = new HomeController(interaction, EnvironmentNamed("Development"), Substitute.For<ILogger<HomeController>>());

        var result = await controller.Error("error-1", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as ErrorViewModel;
        Assert.AreEqual("a real description", vm?.Error?.ErrorDescription);
    }

    [TestMethod]
    public async Task Error_redacts_ErrorDescription_outside_development()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetErrorContextAsync("error-1", Arg.Any<CancellationToken>())
            .Returns(new ErrorMessage { Error = "invalid_request", ErrorDescription = "a sensitive description" });
        var controller = new HomeController(interaction, EnvironmentNamed("Production"), Substitute.For<ILogger<HomeController>>());

        var result = await controller.Error("error-1", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as ErrorViewModel;
        Assert.IsNull(vm?.Error?.ErrorDescription);
    }

    [TestMethod]
    public async Task Error_leaves_Error_null_when_no_error_context_found()
    {
        var interaction = Substitute.For<IIdentityServerInteractionService>();
        interaction.GetErrorContextAsync("missing", Arg.Any<CancellationToken>())
            .Returns((ErrorMessage?)null);
        var controller = new HomeController(interaction, EnvironmentNamed("Development"), Substitute.For<ILogger<HomeController>>());

        var result = await controller.Error("missing", CancellationToken.None);

        var vm = ((OkObjectResult)result.Result!).Value as ErrorViewModel;
        Assert.IsNull(vm?.Error);
    }
}
