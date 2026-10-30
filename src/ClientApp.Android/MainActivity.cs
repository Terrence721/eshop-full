using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace eShop.ClientApp.Android
{
    [Activity(
        Label = "eShop",
        Theme = "@style/Theme.AppCompat.DayNight.NoActionBar",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity
    {
    }
}

// build fix retry
