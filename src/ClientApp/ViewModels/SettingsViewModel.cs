using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;
using eShop.ClientApp.Services.Location;
using eShop.ClientApp.Services.Settings;
using Location = eShop.ClientApp.Models.Location.Location;

namespace eShop.ClientApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        //Needed if using Android Emulator Locally. See https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services?view=net-maui-8.0#android
        private static string _baseAddress = OperatingSystem.IsAndroid() ? "10.0.2.2" : "localhost";

        private readonly IAppEnvironmentService _appEnvironmentService;
        private readonly ILocationService _locationService;
        private readonly ISettingsService _settingsService;
        private bool _allowGpsLocation;
        private string _gatewayBasketEndpoint = null!;
        private string _gatewayCatalogEndpoint = null!;
        private string _gatewayOrdersEndpoint = null!;
        private string _gpsWarningMessage;
        private string _identityEndpoint;
        private double _latitude;
        private double _longitude;

        private bool _useAzureServices;
        private bool _useFakeLocation;

        public SettingsViewModel(
            ILocationService locationService, IAppEnvironmentService appEnvironmentService,
            INavigationService navigationService, ISettingsService settingsService)
            : base(navigationService)
        {
            _settingsService = settingsService;
            _locationService = locationService;
            _appEnvironmentService = appEnvironmentService;

            _useAzureServices = !_settingsService.UseMocks;
            _identityEndpoint = _settingsService.IdentityEndpointBase;
            _latitude = double.Parse(_settingsService.Latitude, CultureInfo.CurrentCulture);
            _longitude = double.Parse(_settingsService.Longitude, CultureInfo.CurrentCulture);
            _useFakeLocation = _settingsService.UseFakeLocation;
            _allowGpsLocation = _settingsService.AllowGpsLocation;
            _gpsWarningMessage = string.Empty;

            IdentityEndpoint =
                !string.IsNullOrEmpty(_settingsService.IdentityEndpointBase)
                    ? _settingsService.IdentityEndpointBase
                    : $"https://{_baseAddress}:5243";

            GatewayCatalogEndpoint =
                !string.IsNullOrEmpty(_settingsService.GatewayCatalogEndpointBase)
                    ? _settingsService.GatewayCatalogEndpointBase
                    : $"http://{_baseAddress}:5222";

            GatewayBasketEndpoint =
                !string.IsNullOrEmpty(_settingsService.GatewayBasketEndpointBase)
                    ? _settingsService.GatewayBasketEndpointBase
                    : $"http://{_baseAddress}:5221";

            GatewayOrdersEndpoint =
                !string.IsNullOrEmpty(_settingsService.GatewayOrdersEndpointBase)
                    ? _settingsService.GatewayOrdersEndpointBase
                    : $"http://{_baseAddress}:5224";

            ToggleMockServicesCommand = new RelayCommand(ToggleMockServices);

            ToggleFakeLocationCommand = new RelayCommand(ToggleFakeLocation);

            ToggleSendLocationCommand = new AsyncRelayCommand(ToggleSendLocationAsync);

            ToggleAllowGpsLocationCommand = new RelayCommand(ToggleAllowGpsLocation);

            // Upstream relied on Shell's automatic back button for this; Avalonia has no Shell
            // chrome (see CatalogItemViewModel.DismissCommand for the same gap), so this needs one.
            DismissCommand = new AsyncRelayCommand(async () => await NavigationService.PopAsync());

            UseAzureServices = !_settingsService.UseMocks;
        }

        public string TitleUseAzureServices => "Use Microservices/Containers from eShop";

        public string DescriptionUseAzureServices => !UseAzureServices
            ? "Currently using mock services that are simulated objects that mimic the behavior of real services using a controlled approach. Toggle on to configure the use of microserivces/containers."
            : "When enabling the use of microservices/containers, the app will attempt to use real services deployed as Docker/Kubernetes containers at the specified base endpoint, which will must be reachable through the network.";

        public bool UseAzureServices
        {
            get => _useAzureServices;
            set
            {
                SetProperty(ref _useAzureServices, value);
                UpdateUseAzureServices();
            }
        }

        public string TitleUseFakeLocation => !UseFakeLocation
            ? "Use Real Location"
            : "Use Fake Location";

        public string DescriptionUseFakeLocation => !UseFakeLocation
            ? "When enabling location, the app will attempt to use the location from the device."
            : "Fake Location data is added for marketing campaign testing.";

        public bool UseFakeLocation
        {
            get => _useFakeLocation;
            set
            {
                SetProperty(ref _useFakeLocation, value);
                UpdateFakeLocation();
            }
        }

        public string TitleAllowGpsLocation => !AllowGpsLocation
            ? "GPS Location Disabled"
            : "GPS Location Enabled";

        public string DescriptionAllowGpsLocation => !AllowGpsLocation
            ? "When disabling location, you won't receive location campaigns based upon your location."
            : "When enabling location, you'll receive location campaigns based upon your location.";

        public string GpsWarningMessage
        {
            get => _gpsWarningMessage;
            set => SetProperty(ref _gpsWarningMessage, value);
        }

        public string IdentityEndpoint
        {
            get => _identityEndpoint;
            set
            {
                SetProperty(ref _identityEndpoint, value);
                if (!string.IsNullOrEmpty(value))
                {
                    UpdateIdentityEndpoint();
                }
            }
        }

        public string GatewayCatalogEndpoint
        {
            get => _gatewayCatalogEndpoint;
            set
            {
                SetProperty(ref _gatewayCatalogEndpoint, value);
                if (!string.IsNullOrEmpty(value))
                {
                    UpdateGatewayShoppingEndpoint();
                }
            }
        }

        public string GatewayOrdersEndpoint
        {
            get => _gatewayOrdersEndpoint;
            set
            {
                SetProperty(ref _gatewayOrdersEndpoint, value);
                if (!string.IsNullOrEmpty(value))
                {
                    UpdateGatewayOrdersEndpoint();
                }
            }
        }

        public string GatewayBasketEndpoint
        {
            get => _gatewayBasketEndpoint;
            set
            {
                SetProperty(ref _gatewayBasketEndpoint, value);
                if (!string.IsNullOrEmpty(value))
                {
                    UpdateGatewayBasketEndpoint();
                }
            }
        }

        public double Latitude
        {
            get => _latitude;
            set
            {
                SetProperty(ref _latitude, value);
                UpdateLatitude();
            }
        }

        public double Longitude
        {
            get => _longitude;
            set
            {
                SetProperty(ref _longitude, value);
                UpdateLongitude();
            }
        }

        public bool AllowGpsLocation
        {
            get => _allowGpsLocation;
            set => SetProperty(ref _allowGpsLocation, value);
        }

        public ICommand ToggleMockServicesCommand { get; }

        public ICommand ToggleFakeLocationCommand { get; }

        public ICommand ToggleSendLocationCommand { get; }

        public ICommand ToggleAllowGpsLocationCommand { get; }

        public ICommand DismissCommand { get; }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(AllowGpsLocation))
            {
                UpdateAllowGpsLocation();
            }
        }

        private void ToggleMockServices()
        {
            _appEnvironmentService.UpdateDependencies(!UseAzureServices);

            OnPropertyChanged(nameof(TitleUseAzureServices));
            OnPropertyChanged(nameof(DescriptionUseAzureServices));
        }

        private void ToggleFakeLocation()
        {
            _appEnvironmentService.UpdateDependencies(!UseAzureServices);
            OnPropertyChanged(nameof(TitleUseFakeLocation));
            OnPropertyChanged(nameof(DescriptionUseFakeLocation));
        }

        private async Task ToggleSendLocationAsync()
        {
            if (!_settingsService.UseMocks)
            {
                var locationRequest = new Location { Latitude = _latitude, Longitude = _longitude };

                await _locationService.UpdateUserLocation(locationRequest);
            }
        }

        private void ToggleAllowGpsLocation()
        {
            OnPropertyChanged(nameof(TitleAllowGpsLocation));
            OnPropertyChanged(nameof(DescriptionAllowGpsLocation));
        }

        private void UpdateUseAzureServices()
        {
            // Save use mocks services to local storage
            _settingsService.UseMocks = !UseAzureServices;

            // Upstream's real View bound this toggle's Checked AND its ToggleMockServicesCommand
            // together on the same control, so every toggle fired both UpdateUseAzureServices() (save)
            // and ToggleMockServices() (swap the live service references) at once. This View only
            // binds the value (IsChecked, see SettingsView.axaml), so without this the switch would
            // only take effect on the next app launch, not the current session -- exactly the gap
            // that left a live app running against the real (unreachable) IdentityService after a
            // quick toggle-on/toggle-off test here, crashing Login with an unhandled connection error.
            _appEnvironmentService.UpdateDependencies(!UseAzureServices);
        }

        private void UpdateIdentityEndpoint()
        {
            // Update remote endpoint (save to local storage)
            _settingsService.IdentityEndpointBase = _identityEndpoint;
        }

        private void UpdateGatewayShoppingEndpoint()
        {
            _settingsService.GatewayCatalogEndpointBase = _gatewayCatalogEndpoint;
        }

        private void UpdateGatewayOrdersEndpoint()
        {
            _settingsService.GatewayOrdersEndpointBase = _gatewayOrdersEndpoint;
        }

        private void UpdateGatewayBasketEndpoint()
        {
            _settingsService.GatewayBasketEndpointBase = _gatewayBasketEndpoint;
        }

        private void UpdateFakeLocation()
        {
            _settingsService.UseFakeLocation = _useFakeLocation;
        }

        private void UpdateLatitude()
        {
            // Update fake latitude (save to local storage)
            _settingsService.Latitude = _latitude.ToString();
        }

        private void UpdateLongitude()
        {
            // Update fake longitude (save to local storage)
            _settingsService.Longitude = _longitude.ToString();
        }

        // MAUI's Permissions API (Permissions.CheckStatusAsync/RequestAsync<...>) has no Avalonia
        // equivalent -- a real GPS permission prompt needs separate native code per platform
        // (Android's ActivityCompat, iOS's CLLocationManager, no concept at all on Desktop), out of
        // scope for this port. Saves the toggle directly instead of pretending to request a
        // permission that isn't actually wired up on any platform yet -- a known, flagged gap,
        // same category as DialogService's modal alert and OpenUrlService's mobile/browser support.
        private void UpdateAllowGpsLocation()
        {
            _settingsService.AllowGpsLocation = _allowGpsLocation;
            GpsWarningMessage = string.Empty;
        }
    }
}
