namespace Webhooks.API.Model
{
    /// <summary>
    /// Serialized webhook payload sent to subscribers.
    /// </summary>
    /// <remarks>
    /// Creates a <see cref="WebhookData" /> from a hook type and event payload.
    /// </remarks>
    public class WebhookData(WebhookType hookType, object data)
    {
        /// <summary>
        /// Timestamp (UTC) the payload was created.
        /// </summary>
        public DateTime When { get; } = DateTime.UtcNow;

        /// <summary>
        /// JSON-serialized webhook payload body.
        /// </summary>
        public string Payload { get; } = JsonSerializer.Serialize(data);

        /// <summary>
        /// String name of the webhook event type.
        /// </summary>
        public string Type { get; } = hookType.ToString();
    }
}
