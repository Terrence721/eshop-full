namespace IdentityServerHost.Quickstart.UI;

// Response shape for GET /Device/Index - either the browser arrived with no
// user code yet (NeedsUserCode, the SPA shows an entry form), or a code was
// supplied and the confirmation form is ready (ViewModel). A missing/expired
// code returns 404 instead, so there's no third state to represent here.
public class DeviceIndexResult
{
    public bool NeedsUserCode { get; set; }
    public DeviceAuthorizationViewModel? ViewModel { get; set; }
}
