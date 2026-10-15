using eShop.ClientApp.Models.Location;
using eShop.ClientApp.Services;

namespace eShop.ClientApp.ViewModels;

// Marketing/Map feature is dead even in upstream - no live service call anywhere,
// this store list is a single static pin. Ported as-is, not wired up. The old MAUI version
// used Microsoft.Maui.Devices.Sensors.Location (a real GPS coordinate type with a (lat, lng)
// constructor) -- no Avalonia equivalent, so this uses our own Models.Location.Location
// instead, via object-initializer syntax since it has no matching constructor.
public partial class MapViewModel(INavigationService navigationService) : ViewModelBase(navigationService)
{
    [ObservableProperty] public partial IEnumerable<Store> Stores { get; set; }

    public override Task InitializeAsync()
    {
        Stores =
            [
                new Store
                {
                    Address = "Building 92, Redmond, WA",
                    Description = "Microsoft Visitor Center",
                    Location = new Location {Latitude = 47.6423109, Longitude = -122.1368406}
                }
            ];

        return Task.CompletedTask;
    }
}

public record Store
{
    public Location Location { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Description { get; set; } = null!;
}
