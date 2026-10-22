using System.Text.Json;

namespace eShop.WebhookClient.Services;

/// <summary>
/// Deserialized webhook payload stored/displayed by the WebhookClient.
/// </summary>
public class WebhookData
{
    /// <summary>
    /// Timestamp (UTC) the payload was received.
    /// </summary>
    public DateTime When { get; set; }

    /// <summary>
    /// JSON-serialized payload body.
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// String name of the webhook event type.
    /// </summary>
    public string? Type { get; set; }
}