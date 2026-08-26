namespace IdentityServerHost.Quickstart.UI;

// Response shape for POST /Device/Callback's redisplay-with-error outcome -
// DeviceController's own doc comment on this branch says it redisplays "the
// rebuilt form with the validation error", but returning the bare
// DeviceAuthorizationViewModel never actually carried ValidationError to the
// caller. Same reasoning as ConsentPostResult/DeviceIndexResult: a dedicated
// wrapper rather than bolting a POST-only field onto the GET-shaped view model.
public class DeviceCallbackResult
{
    public string? ValidationError { get; set; }
    public DeviceAuthorizationViewModel? ViewModel { get; set; }
}
