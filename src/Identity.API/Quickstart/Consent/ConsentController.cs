using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// This controller processes the consent UI
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
[SecurityHeaders]
[Authorize]
public class ConsentController : ControllerBase
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<ConsentController> _logger;

    public ConsentController(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<ConsentController> logger)
    {
        _interaction = interaction;
        _events = events;
        _logger = logger;
    }

    /// <summary>
    /// Shows the consent screen
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ConsentViewModel>> Index(string returnUrl, CancellationToken cancellationToken)
    {
        var vm = await BuildViewModelAsync(returnUrl, cancellationToken: cancellationToken);
        if (vm != null)
        {
            return Ok(vm);
        }

        return NotFound();
    }

    /// <summary>
    /// Handles the consent screen postback
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConsentPostResult>> Index(ConsentInputModel model, CancellationToken cancellationToken)
    {
        var result = await ProcessConsent(model, cancellationToken);

        if (result.IsRedirect)
        {
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl, cancellationToken);
            return Ok(new ConsentPostResult
            {
                RedirectUrl = result.RedirectUri,
                IsNativeClient = context?.IsNativeClient() == true
            });
        }

        if (result.ShowView)
        {
            return Ok(new ConsentPostResult
            {
                ValidationError = result.ValidationError,
                ViewModel = result.ViewModel
            });
        }

        return NotFound();
    }

    /*****************************************/
    /* helper APIs for the ConsentController */
    /*****************************************/
    private async Task<ProcessConsentResult> ProcessConsent(ConsentInputModel model, CancellationToken cancellationToken)
    {
        var result = new ProcessConsentResult();

        // validate return url is still valid
        var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl, cancellationToken);
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
            await _interaction.GrantConsentAsync(request, grantedConsent, cancellationToken);

            // indicate that's it ok to redirect back to authorization endpoint
            result.RedirectUri = model.ReturnUrl;
            result.Client = request.Client;
        }
        else
        {
            // we need to redisplay the consent UI
            result.ViewModel = await BuildViewModelAsync(model.ReturnUrl, model, cancellationToken);
        }

        return result;
    }

    private async Task<ConsentViewModel?> BuildViewModelAsync(string? returnUrl, ConsentInputModel? model = null, CancellationToken cancellationToken = default)
    {
        var request = await _interaction.GetAuthorizationContextAsync(returnUrl, cancellationToken);
        if (request != null)
        {
            return CreateConsentViewModel(model, returnUrl, request);
        }
        else
        {
            _logger.LogError("No consent request matching request: {ReturnUrl}", returnUrl?.ReplaceLineEndings("_"));
        }

        return null;
    }

    private ConsentViewModel CreateConsentViewModel(
        ConsentInputModel? model, string? returnUrl,
        AuthorizationRequest request)
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

        return new ConsentViewModel
        {
            RememberConsent = model?.RememberConsent ?? true,
            ScopesConsented = scopesConsented,
            Description = model?.Description,

            ReturnUrl = returnUrl,

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
        var displayName = apiScope.DisplayName ?? apiScope.Name;
        if (!string.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
        {
            displayName += ":" + parsedScopeValue.ParsedParameter;
        }

        return new ScopeViewModel
        {
            Value = parsedScopeValue.RawValue,
            DisplayName = displayName,
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
