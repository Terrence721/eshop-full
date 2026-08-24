namespace IdentityServerHost.Quickstart.UI;

public class DeviceAuthorizationInputModel : ConsentInputModel
{
    public required string UserCode { get; set; }
}
