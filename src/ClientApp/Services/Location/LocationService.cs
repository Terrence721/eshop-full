using eShop.ClientApp.Services.Identity;

namespace eShop.ClientApp.Services.Location
{
    // Upstream's own implementation is incomplete - the real POST call is commented out
    // and references a _requestProvider field that was never added to this class.
    public class LocationService(IIdentityService identityService) : ILocationService
    {
        private const string ApiUrlBase = "l/api/v1/locations";
        private readonly IIdentityService _identityService = identityService;

        public async Task UpdateUserLocation(Models.Location.Location newLocReq)
        {
            var accessToken = await _identityService.GetAuthTokenAsync().ConfigureAwait(false);

            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            //TODO: Determine mapped location
            await Task.Delay(10).ConfigureAwait(false);
            //await _requestProvider.PostAsync(uri, newLocReq, token).ConfigureAwait(false);
        }
    }
}
