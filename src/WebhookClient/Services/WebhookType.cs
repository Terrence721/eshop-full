namespace eShop.WebhookClient.Services;

/// <summary>
/// Identifies the kind of webhook event a subscription targets.
/// </summary>
public enum WebhookType
{
    /// <summary>A product's price has changed.</summary>
    CatalogItemPriceChange = 1,

    /// <summary>An order has shipped.</summary>
    OrderShipped = 2,

    /// <summary>An order has been paid.</summary>
    OrderPaid = 3
}