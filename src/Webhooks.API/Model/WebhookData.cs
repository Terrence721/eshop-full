namespace Webhooks.API.Model;

/// <summary>
/// Serialized webhook payload sent to subscribers.
/// </summary>
public class WebhookData
{
    /// <summary>
    /// Timestamp (UTC) the payload was created.
    /// </summary>
    public DateTime When { get; }

    /// <summary>
    /// JSON-serialized webhook payload body.
    /// </summary>
    public string Payload { get; }

    /// <summary>
    /// String name of the webhook event type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Creates a <see cref="WebhookData" /> from a hook type and event payload.
    /// </summary>
    public WebhookData(WebhookType hookType, object data)
    {
        When = DateTime.UtcNow;
        Type = hookType.ToString();
        Payload = JsonSerializer.Serialize(data);
    }
}