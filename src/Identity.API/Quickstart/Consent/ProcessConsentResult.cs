using Duende.IdentityServer.Models;

namespace IdentityServerHost.Quickstart.UI;

public class ProcessConsentResult<TViewModel> where TViewModel : ConsentScopesViewModel
{
    public bool IsRedirect => RedirectUri != null;
    public string? RedirectUri { get; set; }
    public Client? Client { get; set; }

    public bool ShowView => ViewModel != null;
    public TViewModel? ViewModel { get; set; }

    public bool HasValidationError => ValidationError != null;
    public string? ValidationError { get; set; }
}
