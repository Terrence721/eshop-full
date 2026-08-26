using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerHost.Quickstart.UI;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
[SecurityHeaders]
public class DeviceController : ControllerBase
{
    private readonly IDeviceFlowInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(
        IDeviceFlowInteractionService interaction,
        IEventService eventService,
        ILogger<DeviceController> logger)
    {
        _interaction = interaction;
        _events = eventService;
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
            // ConsentViewModel? (also used by ConsentController), but
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

        ConsentResponse? grantedConsent = null;

        // user clicked 'no' - send back the standard 'access_denied' response
        if (model.Button == "no")
        {
            grantedConsent = new ConsentResponse { Error = InteractionError.AccessDenied };

            // emit event
            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues), cancellationToken);
        }
        // user clicked 'yes' - validate the data
        else if (model.Button == "yes")
        {
            // if the user consented to some scope, build the response model
            if (model.ScopesConsented != null && model.ScopesConsented.Any())
            {
                // ConsentOptions.EnableOfflineAccess is const true, so filtering
                // offline_access out here (upstream's original behavior when it's
                // disabled) is unreachable - removed rather than kept as dead code.
                grantedConsent = new ConsentResponse
                {
                    RememberConsent = model.RememberConsent,
                    ScopesValuesConsented = model.ScopesConsented.ToArray(),
                    Description = model.Description
                };

                // emit event
                await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent), cancellationToken);
            }
            else
            {
                result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
            }
        }
        else
        {
            result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;
        }

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
            .Select(x => CreateScopeViewModel(x, scopesConsented.Contains(x.Name) || model == null))
            .ToArray();

        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = CreateScopeViewModel(parsedScope, apiScope, scopesConsented.Contains(parsedScope.RawValue) || model == null);
                apiScopes.Add(scopeVm);
            }
        }
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(scopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || model == null));
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

    private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
        return new ScopeViewModel
        {
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            Checked = check || identity.Required
        };
    }

    private ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        return new ScopeViewModel
        {
            Value = parsedScopeValue.RawValue,
            DisplayName = apiScope.DisplayName ?? apiScope.Name,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
    }

    private ScopeViewModel GetOfflineAccessScope(bool check)
    {
        return new ScopeViewModel
        {
            Value = IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = ConsentOptions.OfflineAccessDisplayName,
            Description = ConsentOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}
