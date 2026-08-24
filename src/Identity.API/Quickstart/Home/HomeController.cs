using System.Diagnostics;
using Duende.IdentityServer.Hosting;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerHost.Quickstart.UI;

[ApiController]
[Route("[controller]/[action]")]
[SecurityHeaders]
[AllowAnonymous]
public class HomeController : ControllerBase
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    public HomeController(
        IIdentityServerInteractionService interaction,
        IWebHostEnvironment environment,
        ILogger<HomeController> logger)
    {
        _interaction = interaction;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IndexViewModel> Index()
    {
        if (_environment.IsDevelopment())
        {
            // only show in development
            var version = FileVersionInfo.GetVersionInfo(typeof(IdentityServerMiddleware).Assembly.Location).ProductVersion?.Split('+').First() ?? "unknown";
            return Ok(new IndexViewModel
            {
                Version = version,
                WellKnownConfigurationUrl = "/.well-known/openid-configuration",
                DiagnosticsUrl = "/diagnostics",
                GrantsUrl = "/grants"
            });
        }

        _logger.LogInformation("Homepage is disabled in production. Returning 404.");
        return NotFound();
    }

    /// <summary>
    /// Shows the error page
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ErrorViewModel>> Error(string errorId, CancellationToken cancellationToken)
    {
        var vm = new ErrorViewModel();

        // retrieve error details from identityserver
        var message = await _interaction.GetErrorContextAsync(errorId, cancellationToken);
        if (message != null)
        {
            vm.Error = message;

            if (!_environment.IsDevelopment())
            {
                // only show in development
                message.ErrorDescription = null;
            }
        }

        return Ok(vm);
    }
}
