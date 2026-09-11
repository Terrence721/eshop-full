using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// Shared by ConsentController and DeviceController -- both decide whether a
/// posted consent decision was granted, denied, or invalid the same way,
/// differing only in which interaction service finalizes the outcome and how
/// each controller shapes its own result type. Composition over a shared
/// base controller: the two controllers use different interaction service
/// types (IIdentityServerInteractionService vs IDeviceFlowInteractionService)
/// and different result semantics (IsRedirect vs Client != null), so a base
/// class here would need to abstract over both anyway -- an injected
/// collaborator is the more direct fit, matching ScopeViewModelFactory's
/// precedent for the scope-list-building half of this same duplication.
/// </summary>
public class ConsentResponseBuilder(IEventService events)
{
    public async Task<(ConsentResponse? GrantedConsent, string? ValidationError)> BuildAsync(
        ConsentDecisionInputModel model,
        string? subjectId,
        string clientId,
        IEnumerable<string> rawScopeValues,
        CancellationToken cancellationToken)
    {
        // user clicked 'no' - send back the standard 'access_denied' response
        if (model.Button == "no")
        {
            var deniedConsent = new ConsentResponse { Error = InteractionError.AccessDenied };

            await events.RaiseAsync(new ConsentDeniedEvent(subjectId, clientId, rawScopeValues), cancellationToken);

            return (deniedConsent, null);
        }

        // user clicked 'yes' - validate the data
        if (model.Button == "yes")
        {
            // if the user consented to some scope, build the response model
            if (model.ScopesConsented != null && model.ScopesConsented.Any())
            {
                // ConsentOptions.EnableOfflineAccess is const true, so filtering
                // offline_access out here (upstream's original behavior when it's
                // disabled) is unreachable - removed rather than kept as dead code.
                var grantedConsent = new ConsentResponse
                {
                    RememberConsent = model.RememberConsent,
                    ScopesValuesConsented = model.ScopesConsented.ToArray(),
                    Description = model.Description
                };

                await events.RaiseAsync(new ConsentGrantedEvent(subjectId, clientId, rawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent), cancellationToken);

                return (grantedConsent, null);
            }

            return (null, ConsentOptions.MustChooseOneErrorMessage);
        }

        return (null, ConsentOptions.InvalidSelectionErrorMessage);
    }
}
