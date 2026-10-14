namespace eShop.ClientApp.Models.Marketing;

// Marketing/Map feature is dead even in upstream - no live service call anywhere,
// MapViewModel's store list is a single static pin. Ported as-is, not wired up.
public class Campaign
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public string PictureUri { get; set; } = null!;
}
