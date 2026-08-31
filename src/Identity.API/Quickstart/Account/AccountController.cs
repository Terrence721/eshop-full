using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using eShop.Identity.API.Models;

namespace IdentityServerHost.Quickstart.UI;

[ApiController]
[Route("[controller]/[action]")]
[SecurityHeaders]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IAuthenticationHandlerProvider _handlerProvider;
    private readonly IEventService _events;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IClientStore clientStore,
        IAuthenticationSchemeProvider schemeProvider,
        IAuthenticationHandlerProvider handlerProvider,
        IEventService events)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _clientStore = clientStore;
        _schemeProvider = schemeProvider;
        _handlerProvider = handlerProvider;
        _events = events;
    }

    /// <summary>
    /// Entry point into the login workflow
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<LoginViewModel>> Login(string? returnUrl, CancellationToken cancellationToken)
    {
        var vm = await BuildLoginViewModelAsync(returnUrl, cancellationToken);
        return Ok(vm);
    }

    /// <summary>
    /// Handle postback from username/password login. Split from the cancel flow
    /// (see LoginCancel) rather than dispatched via a shared "which button did you
    /// click" form field - that pattern only existed because a single Razor
    /// &lt;form&gt; can have two submit buttons sharing one postback target. A JSON
    /// API has no such constraint, and the split also resolves a real CodeQL
    /// finding (cs/user-controlled-bypass, alert #7): a user-controlled value was
    /// gating whether IIdentityServerInteractionService.DenyAuthorizationAsync
    /// ran. It's not reachable by any value now, ever, from this action.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LoginPostResult>> Login(LoginInputModel model, CancellationToken cancellationToken)
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl, cancellationToken);

        // [ApiController] returns an automatic 400 for a ModelState made invalid by
        // binding/[Required] before this action ever runs, so this is always true by
        // the time we reach it - kept for fidelity with the upstream Quickstart, not
        // because it can actually be false here.
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName ?? model.Username, user.Id, user.UserName ?? model.Username, clientId: context?.Client.ClientId), cancellationToken);
                }

                if (context != null)
                {
                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Ok(new LoginPostResult
                    {
                        RedirectUrl = model.ReturnUrl,
                        IsNativeClient = context.IsNativeClient()
                    });
                }

                // request for a local page
                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Ok(new LoginPostResult { RedirectUrl = model.ReturnUrl });
                }
                else if (string.IsNullOrEmpty(model.ReturnUrl))
                {
                    return Ok(new LoginPostResult { RedirectUrl = "~/" });
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    throw new Exception("invalid return URL");
                }
            }

            await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId), cancellationToken);
        }

        // something went wrong, redisplay form with error. Given the always-true
        // ModelState.IsValid above, the only real path here is invalid credentials,
        // so ValidationError can be set unconditionally rather than tracked through
        // a separate flag.
        var vm = await BuildLoginViewModelAsync(model, cancellationToken);
        return Ok(new LoginPostResult { ViewModel = vm, ValidationError = AccountOptions.InvalidCredentialsErrorMessage });
    }

    /// <summary>
    /// Handle the user clicking "cancel" on the login page - split out from Login
    /// (see its doc comment) since there's no longer a shared postback target to
    /// dispatch on.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LoginPostResult>> LoginCancel(string? returnUrl, CancellationToken cancellationToken)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl, cancellationToken);
        if (context != null)
        {
            // if the user cancels, send a result back into IdentityServer as if they
            // denied the consent (even if this client does not require consent).
            // this will send back an access denied OIDC error response to the client.
            await _interaction.DenyAuthorizationAsync(context, InteractionError.AccessDenied, cancellationToken);

            // we can trust returnUrl since GetAuthorizationContextAsync returned non-null
            return Ok(new LoginPostResult
            {
                RedirectUrl = returnUrl,
                IsNativeClient = context.IsNativeClient()
            });
        }

        // since we don't have a valid context, then we just go back to the home page
        return Ok(new LoginPostResult { RedirectUrl = "~/" });
    }

    /// <summary>
    /// Show logout page
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<LogoutViewModel>> Logout(string? logoutId, CancellationToken cancellationToken)
    {
        // build a model so the caller knows what to display
        var vm = await BuildLogoutViewModelAsync(logoutId, cancellationToken);
        return Ok(vm);
    }

    /// <summary>
    /// Handle logout page postback. Always redirects rather than ever
    /// returning the LoggedOutViewModel directly -- a caller can't know in
    /// advance whether this ends in a same-origin redirect (the common
    /// case) or a genuine cross-origin one (external-IdP sign-out), and
    /// only a real redirect lets the browser handle either uniformly.
    /// fetch() follows a same-origin redirect transparently; a cross-origin
    /// one needs a real full-page POST, which only the browser can do --
    /// which is also why logoutId is a plain query parameter here (matching
    /// LoginCancel below) rather than a LogoutInputModel JSON body: a real
    /// &lt;form&gt; submission can only send application/x-www-form-urlencoded,
    /// which [FromBody] JSON binding rejects outright (confirmed for real:
    /// a form-encoded POST against the JSON-bound version came back 415).
    /// Named LogoutPost, not Logout -- now that both actions share the
    /// identical (string?, CancellationToken) signature, C# overload
    /// resolution (unaware of [HttpGet]/[HttpPost]) can't tell them apart
    /// by method name alone. [ActionName] keeps the route /Account/Logout.
    /// </summary>
    [HttpPost]
    [ActionName("Logout")]
    public async Task<IActionResult> LogoutPost(string? logoutId, CancellationToken cancellationToken)
    {
        // build a model so we know whether an external IdP is involved
        var vm = await BuildLoggedOutViewModelAsync(logoutId, cancellationToken);

        if (User.Identity?.IsAuthenticated == true)
        {
            // delete local authentication cookie
            await _signInManager.SignOutAsync();

            // raise the logout event
            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()), cancellationToken);
        }

        // check if we need to trigger sign-out at an upstream identity provider
        // (checked via ExternalAuthenticationScheme directly, rather than
        // vm.TriggerExternalSignout, so the compiler can narrow it non-null below)
        if (vm.ExternalAuthenticationScheme != null)
        {
            // build a return URL so the upstream provider will redirect back
            // to us after the user has logged out. this allows us to then
            // complete our single sign-out processing.
            var url = Url.Action(nameof(LoggedOut), new { logoutId = vm.LogoutId });

            // this triggers a redirect to the external provider for sign-out
            return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
        }

        return RedirectToAction(nameof(LoggedOut), new { logoutId = vm.LogoutId });
    }

    /// <summary>
    /// Show the logged-out confirmation. The one real landing point for both
    /// of Logout(POST)'s outcomes -- the direct case redirects here itself,
    /// the external-IdP case lands here after the upstream provider's own
    /// sign-out redirects back.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<LoggedOutViewModel>> LoggedOut(string? logoutId, CancellationToken cancellationToken)
    {
        var vm = await BuildLoggedOutViewModelAsync(logoutId, cancellationToken);
        return Ok(vm);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return NoContent();
    }

    /*****************************************/
    /* helper APIs for the AccountController */
    /*****************************************/
    private async Task<LoginViewModel> BuildLoginViewModelAsync(string? returnUrl, CancellationToken cancellationToken)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl, cancellationToken);
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

            // this is meant to short circuit the UI and only trigger the one external IdP
            var vm = new LoginViewModel
            {
                EnableLocalLogin = local,
                ReturnUrl = returnUrl,
                Username = context.LoginHint,
            };

            if (!local)
            {
                vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
            }

            return vm;
        }

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ExternalProvider
            {
                DisplayName = x.DisplayName ?? x.Name,
                AuthenticationScheme = x.Name
            }).ToList();

        var allowLocal = true;
        if (context?.Client.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId, cancellationToken);
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;

                if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                {
                    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                }
            }
        }

        return new LoginViewModel
        {
            AllowRememberLogin = AccountOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
            ReturnUrl = returnUrl,
            Username = context?.LoginHint,
            ExternalProviders = providers.ToArray()
        };
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model, CancellationToken cancellationToken)
    {
        var vm = await BuildLoginViewModelAsync(model.ReturnUrl, cancellationToken);
        vm.Username = model.Username;
        vm.RememberLogin = model.RememberLogin;
        return vm;
    }

    private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string? logoutId, CancellationToken cancellationToken)
    {
        var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

        if (User.Identity?.IsAuthenticated != true)
        {
            // if the user is not authenticated, then just show logged out page
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        var context = await _interaction.GetLogoutContextAsync(logoutId, cancellationToken);
        if (context?.ShowSignoutPrompt == false)
        {
            // it's safe to automatically sign-out
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        // show the logout prompt. this prevents attacks where the user
        // is automatically signed out by another malicious web page.
        return vm;
    }

    private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string? logoutId, CancellationToken cancellationToken)
    {
        // get context information (client name, post logout redirect URI and iframe for federated signout)
        var logout = await _interaction.GetLogoutContextAsync(logoutId, cancellationToken);

        var vm = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
            ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            SignOutIframeUrl = logout?.SignOutIFrameUrl,
            LogoutId = logoutId
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                var handler = await _handlerProvider.GetHandlerAsync(HttpContext, idp);
                if (handler is IAuthenticationSignOutHandler)
                {
                    if (vm.LogoutId == null)
                    {
                        // if there's no current logout context, we need to create one
                        // this captures necessary info from the current logged in user
                        // before we signout and redirect away to the external IdP for signout
                        vm.LogoutId = await _interaction.CreateLogoutContextAsync(cancellationToken);
                    }

                    vm.ExternalAuthenticationScheme = idp;
                }
            }
        }

        return vm;
    }
}
