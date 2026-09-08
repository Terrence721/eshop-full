using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerHost.Quickstart.UI;

[Authorize]
public class DeviceController : QuickstartControllerBase
{
    private readonly IDeviceFlowInteractionService _interaction;
    private readonly ConsentResponseBuilder _responseBuilder;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(
        IDeviceFlowInteractionService interaction,
        ConsentResponseBuilder responseBuilder,
        ILogger<DeviceController> logger)
    {
        _interaction = interaction;
        _responseBuilder = responseBuilder;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<DeviceIndexResult>> Index([FromQuery(Name = "userCode")] string? userCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return Ok(new DeviceIndexResult { NeedsUserCode = true });
        }

        var vm = await BuildViewModelAsync(userCode, cancellationToken: cancellationToken);
        if (vm == null)
        {
            return NotFound();
        }

        vm.ConfirmUserCode = true;
        return Ok(new DeviceIndexResult { ViewModel = vm });
    }

    [HttpPost]
    public async Task<ActionResult<DeviceAuthorizationViewModel>> UserCodeCapture(string userCode, CancellationToken cancellationToken)
    {
        var vm = await BuildViewModelAsync(userCode, cancellationToken: cancellationToken);
        if (vm == null)
        {
            return NotFound();
        }

        return Ok(vm);
    }

    [HttpPost]
    public async Task<ActionResult<DeviceCallbackResult>> Callback(DeviceAuthorizationInputModel model, CancellationToken cancellationToken)
    {
        var result = await ProcessConsent(model, cancellationToken);

        if (result.ShowView)
        {
            // Unified with ConsentController's pattern: redisplay the rebuilt form
            // with the validation error instead of a generic failure. Upstream fell
            // back to a generic error page here regardless of validation vs. hard
            // failure, unlike ConsentController, which already redisplayed the form.
            // ProcessConsentResult.ViewModel is typed as the shared base
            // ConsentScopesViewModel? (also used by ConsentController), but
            // BuildViewModelAsync below only ever assigns it a real
            // DeviceAuthorizationViewModel - safe to cast back.
            return Ok(new DeviceCallbackResult
            {
                ValidationError = result.ValidationError,
                ViewModel = (DeviceAuthorizationViewModel?)result.ViewModel
            });
        }

        if (result.Client != null)
        {
            // grantedConsent != null in ProcessConsent - consent was actually granted.
            // Device flow has no client to redirect back to (the device itself polls
            // the token endpoint separately), so unlike ConsentController this isn't
            // IsRedirect - Client is the real "succeeded" signal here.
            return NoContent();
        }

        // ProcessConsent found no matching device-flow authorization for this UserCode
        // (e.g. expired between page load and submit). Upstream's original code fell
        // through to the same "Success" view here, silently treating "not found" as
        // success - a real bug, not reproduced.
        return NotFound();
    }

    private async Task<ProcessConsentResult> ProcessConsent(DeviceAuthorizationInputModel model, CancellationToken cancellationToken)
    {
        var result = new ProcessConsentResult();

        var request = await _interaction.GetAuthorizationContextAsync(model.UserCode, cancellationToken);
        if (request == null) return result;

        var (grantedConsent, validationError) = await _responseBuilder.BuildAsync(
            model, User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, cancellationToken);
        result.ValidationError = validationError;

        if (grantedConsent != null)
        {
            // communicate outcome of consent back to identityserver
            await _interaction.HandleRequestAsync(model.UserCode, grantedConsent, cancellationToken);

            result.Client = request.Client;
        }
        else
        {
            // we need to redisplay the consent UI
            result.ViewModel = await BuildViewModelAsync(model.UserCode, model, cancellationToken);
        }

        return result;
    }

    private async Task<DeviceAuthorizationViewModel?> BuildViewModelAsync(string userCode, DeviceAuthorizationInputModel? model = null, CancellationToken cancellationToken = default)
    {
        var request = await _interaction.GetAuthorizationContextAsync(userCode, cancellationToken);
        if (request != null)
        {
            return CreateConsentViewModel(userCode, model, request);
        }

        return null;
    }

    private DeviceAuthorizationViewModel CreateConsentViewModel(string userCode, DeviceAuthorizationInputModel? model, DeviceFlowAuthorizationRequest request)
    {
        var scopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>();

        var identityScopes = request.ValidatedResources.Resources.IdentityResources
            .Select(x => ScopeViewModelFactory.CreateScopeViewModel(x, scopesConsented.Contains(x.Name) || model == null))
            .ToArray();

        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = ScopeViewModelFactory.CreateScopeViewModel(parsedScope, apiScope, scopesConsented.Contains(parsedScope.RawValue) || model == null);
                apiScopes.Add(scopeVm);
            }
        }
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(ScopeViewModelFactory.GetOfflineAccessScope(scopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
        }

        return new DeviceAuthorizationViewModel
        {
            UserCode = userCode,
            Description = model?.Description,

            RememberConsent = model?.RememberConsent ?? true,
            ScopesConsented = scopesConsented,

            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent,

            IdentityScopes = identityScopes,
            ApiScopes = apiScopes
        };
    }
}
