using System.Globalization;
using System.Text.Json;
using eShop.ClientApp.Models.Token;

namespace eShop.ClientApp.Services.Settings;

public class SettingsService : ISettingsService
{
    #region Setting Constants

    private const string UserAccessToken = "user_token";
    private const string IdUseMocks = "use_mocks";
    private const string IdIdentityBase = "url_base";
    private const string DefaultClientId = "maui";
    private const string DefaultClientSecret = "secret";
    private const string DefaultCallbackUri = "maui://authcallback";
    private const string IdGatewayShoppingBase = "url_shopping";
    private const string IdGatewayOrdersBase = "url_orders";
    private const string IdGatewayBasketBase = "url_basket";
    private const string IdUseFakeLocation = "use_fake_location";
    private const string IdLatitude = "latitude";
    private const string IdLongitude = "longitude";
    private const string IdAllowGpsLocation = "allow_gps_location";
    private readonly bool UseMocksDefault = true;
    private readonly bool UseFakeLocationDefault = false;
    private readonly bool AllowGpsLocationDefault = false;
    private readonly double FakeLatitudeDefault = 47.604610d;
    private readonly double FakeLongitudeDefault = -122.315752d;

    #endregion

    #region Settings Properties

    public async Task SetUserTokenAsync(UserToken? userToken)
    {
        LocalSettingsStore.Set(
            UserAccessToken,
            userToken is not null ? JsonSerializer.Serialize(userToken, EShopJsonSerializerContext.Default.UserToken) : string.Empty);
        await Task.CompletedTask;
    }

    public async Task<UserToken?> GetUserTokenAsync()
    {
        var userToken = LocalSettingsStore.Get(UserAccessToken);
        await Task.CompletedTask;

        return string.IsNullOrEmpty(userToken) ? default : JsonSerializer.Deserialize(userToken, EShopJsonSerializerContext.Default.UserToken);
    }

    public bool UseMocks
    {
        get => GetBool(IdUseMocks, UseMocksDefault);
        set => SetBool(IdUseMocks, value);
    }

    public string DefaultEndpoint
    {
        get => LocalSettingsStore.Get(nameof(DefaultEndpoint)) ?? string.Empty;
        set => LocalSettingsStore.Set(nameof(DefaultEndpoint), value);
    }

    public string RegistrationEndpoint
    {
        get => LocalSettingsStore.Get(nameof(RegistrationEndpoint)) ?? string.Empty;
        set => LocalSettingsStore.Set(nameof(RegistrationEndpoint), value);
    }

    public string ClientId
    {
        get => LocalSettingsStore.Get(nameof(ClientId)) ?? DefaultClientId;
        set => LocalSettingsStore.Set(nameof(ClientId), value);
    }

    public string ClientSecret
    {
        get => LocalSettingsStore.Get(nameof(ClientSecret)) ?? DefaultClientSecret;
        set => LocalSettingsStore.Set(nameof(ClientSecret), value);
    }

    public string CallbackUri
    {
        get => LocalSettingsStore.Get(nameof(CallbackUri)) ?? DefaultCallbackUri;
        set => LocalSettingsStore.Set(nameof(CallbackUri), value);
    }

    public string IdentityEndpointBase
    {
        get => LocalSettingsStore.Get(IdIdentityBase) ?? string.Empty;
        set => LocalSettingsStore.Set(IdIdentityBase, value);
    }

    public string GatewayCatalogEndpointBase
    {
        get => LocalSettingsStore.Get(IdGatewayShoppingBase) ?? string.Empty;
        set => LocalSettingsStore.Set(IdGatewayShoppingBase, value);
    }

    public string GatewayOrdersEndpointBase
    {
        get => LocalSettingsStore.Get(IdGatewayOrdersBase) ?? string.Empty;
        set => LocalSettingsStore.Set(IdGatewayOrdersBase, value);
    }

    public string GatewayBasketEndpointBase
    {
        get => LocalSettingsStore.Get(IdGatewayBasketBase) ?? string.Empty;
        set => LocalSettingsStore.Set(IdGatewayBasketBase, value);
    }

    public bool UseFakeLocation
    {
        get => GetBool(IdUseFakeLocation, UseFakeLocationDefault);
        set => SetBool(IdUseFakeLocation, value);
    }

    public string Latitude
    {
        get => LocalSettingsStore.Get(IdLatitude) ?? FakeLatitudeDefault.ToString(CultureInfo.InvariantCulture);
        set => LocalSettingsStore.Set(IdLatitude, value);
    }

    public string Longitude
    {
        get => LocalSettingsStore.Get(IdLongitude) ?? FakeLongitudeDefault.ToString(CultureInfo.InvariantCulture);
        set => LocalSettingsStore.Set(IdLongitude, value);
    }

    public bool AllowGpsLocation
    {
        get => GetBool(IdAllowGpsLocation, AllowGpsLocationDefault);
        set => SetBool(IdAllowGpsLocation, value);
    }

    private static bool GetBool(string key, bool defaultValue)
    {
        var value = LocalSettingsStore.Get(key);
        return value is null ? defaultValue : bool.Parse(value);
    }

    private static void SetBool(string key, bool value)
    {
        LocalSettingsStore.Set(key, value.ToString(CultureInfo.InvariantCulture));
    }

    #endregion
}
