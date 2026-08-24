namespace IdentityServerHost.Quickstart.UI;

public class DeviceAuthorizationViewModel : ConsentViewModel
{
    public required string UserCode { get; set; }
    public bool ConfirmUserCode { get; set; }
}
