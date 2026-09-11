namespace IdentityServerHost.Quickstart.UI;

public class DeviceAuthorizationInputModel : ConsentDecisionInputModel
{
    public required string UserCode { get; set; }
}
