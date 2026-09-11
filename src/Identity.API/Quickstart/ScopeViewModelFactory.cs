using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// Shared by ConsentController and DeviceController -- both build the same
/// consent-screen scope list from an IdentityServer authorization request.
/// </summary>
public static class ScopeViewModelFactory
{
    public static ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
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

    public static ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
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

    public static ScopeViewModel GetOfflineAccessScope(bool check)
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

    /// <summary>
    /// Builds the shared scope-consent fields (<see cref="ConsentScopesViewModel"/>) from an
    /// authorization request. AuthorizationRequest and DeviceFlowAuthorizationRequest share no
    /// common base or interface (verified against Duende.IdentityServer 8.0.7), so this takes
    /// the two properties both callers already have on hand rather than the whole request.
    /// </summary>
    public static ConsentScopesViewModel BuildScopesViewModel(
        ResourceValidationResult validatedResources,
        Client client,
        IEnumerable<string> scopesConsented,
        bool isInitialDisplay,
        bool rememberConsent,
        string? description)
    {
        var identityScopes = validatedResources.Resources.IdentityResources
            .Select(x => CreateScopeViewModel(x, scopesConsented.Contains(x.Name) || isInitialDisplay))
            .ToArray();

        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in validatedResources.ParsedScopes)
        {
            var apiScope = validatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                apiScopes.Add(CreateScopeViewModel(parsedScope, apiScope, scopesConsented.Contains(parsedScope.RawValue) || isInitialDisplay));
            }
        }
        if (ConsentOptions.EnableOfflineAccess && validatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(scopesConsented.Contains(IdentityServerConstants.StandardScopes.OfflineAccess) || isInitialDisplay));
        }

        return new ConsentScopesViewModel
        {
            RememberConsent = rememberConsent,
            ScopesConsented = scopesConsented,
            Description = description,

            ClientName = client.ClientName ?? client.ClientId,
            ClientUrl = client.ClientUri,
            ClientLogoUrl = client.LogoUri,
            AllowRememberConsent = client.AllowRememberConsent,

            IdentityScopes = identityScopes,
            ApiScopes = apiScopes
        };
    }
}
