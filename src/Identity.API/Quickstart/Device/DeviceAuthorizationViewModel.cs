namespace IdentityServerHost.Quickstart.UI;

public class DeviceAuthorizationViewModel : ConsentScopesViewModel
{
    public required string UserCode { get; set; }
    public bool ConfirmUserCode { get; set; }
}
