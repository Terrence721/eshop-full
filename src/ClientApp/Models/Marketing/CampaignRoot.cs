namespace eShop.ClientApp.Models.Marketing;

// Marketing/Map feature is dead even in upstream - no live service call anywhere,
// MapViewModel's store list is a single static pin. Ported as-is, not wired up.
public class CampaignRoot
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }
    public List<CampaignItem> Data { get; set; } = null!;
}
