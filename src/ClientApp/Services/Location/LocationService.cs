namespace eShop.ClientApp.Services.Location
{
    // Upstream's real location POST was never completed (commented out and missing its request
    // provider/endpoint). Until a concrete contract exists this is intentionally a no-op so the
    // settings toggle keeps working without crashing on an unreachable backend.
    public class LocationService : ILocationService
    {
        public Task UpdateUserLocation(Models.Location.Location newLocReq) => Task.CompletedTask;
    }
}