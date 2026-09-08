using Microsoft.AspNetCore.Mvc;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// Every Quickstart controller repeats the same 3 attributes, varying only in
/// [Authorize]/[AllowAnonymous] (which each controller still applies itself).
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
[SecurityHeaders]
public abstract class QuickstartControllerBase : ControllerBase
{
}
