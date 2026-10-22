namespace eShop.WebhookClient.Services;

/// <summary>
/// Response returned for a webhook registration attempt.
/// </summary>
public class WebhookResponse
{
    /// <summary>
    /// Registration/attempt timestamp.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Destination URL that was registered.
    /// </summary>
    public string? DestUrl { get; set; }

    /// <summary>
    /// Token associated with the registration attempt.
    /// </summary>
    public string? Token { get; set; }
}