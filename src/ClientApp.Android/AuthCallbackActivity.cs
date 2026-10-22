using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace eShop.ClientApp.Android
{
    // The intent-filter below is what makes eshop://authcallback redirect here instead of opening in
    // the browser -- the package manager wires this up from the manifest at install time, no runtime
    // registration needed (unlike Desktop's registry-key approach). NoHistory keeps this Activity out
    // of the back stack (there's nothing to show -- it reads the redirect and finishes immediately),
    // and SingleTask means it reuses the existing app task instead of creating a new one, so the user
    // lands back on whatever screen they started the login from rather than a second app instance.
    [Activity(
        NoHistory = true,
        LaunchMode = LaunchMode.SingleTask,
        Exported = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    [IntentFilter(
        [Intent.ActionView],
        Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
        DataScheme = "eshop",
        DataHost = "authcallback")]
    public class AuthCallbackActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var url = Intent?.DataString;

            if (url is not null)
            {
                AndroidAuthenticationBrowser.CompleteLogin(url);
            }

            Finish();
        }
    }
}
