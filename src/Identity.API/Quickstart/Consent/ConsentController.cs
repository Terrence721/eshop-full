using Duende.IdentityServer;
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
[Authorize]
public class ConsentController : QuickstartControllerBase
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ConsentResponseBuilder _responseBuilder;
    private readonly ILogger<ConsentController> _logger;

    public ConsentController(
        IIdentityServerInteractionService interaction,
        ConsentResponseBuilder responseBuilder,
        ILogger<ConsentController> logger)
    {
        _interaction = interaction;
        _responseBuilder = responseBuilder;
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
    private async Task<ProcessConsentResult<ConsentViewModel>> ProcessConsent(ConsentInputModel model, CancellationToken cancellationToken)
    {
        var result = new ProcessConsentResult<ConsentViewModel>();

        // validate return url is still valid
        var request = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl, cancellationToken);
        if (request == null) return result;

        var (grantedConsent, validationError) = await _responseBuilder.BuildAsync(
            model, User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, cancellationToken);
        result.ValidationError = validationError;

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

    private static ConsentViewModel CreateConsentViewModel(
        ConsentInputModel? model, string? returnUrl,
        AuthorizationRequest request)
    {
        var scopesConsented = model?.ScopesConsented ?? Enumerable.Empty<string>();
        var scopes = ScopeViewModelFactory.BuildScopesViewModel(
            request.ValidatedResources, request.Client, scopesConsented, model == null,
            model?.RememberConsent ?? true, model?.Description);

        return new ConsentViewModel
        {
            ReturnUrl = returnUrl,

            RememberConsent = scopes.RememberConsent,
            ScopesConsented = scopes.ScopesConsented,
            Description = scopes.Description,

            ClientName = scopes.ClientName,
            ClientUrl = scopes.ClientUrl,
            ClientLogoUrl = scopes.ClientLogoUrl,
            AllowRememberConsent = scopes.AllowRememberConsent,

            IdentityScopes = scopes.IdentityScopes,
            ApiScopes = scopes.ApiScopes
        };
    }
}
