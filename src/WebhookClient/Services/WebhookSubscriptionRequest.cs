namespace eShop.WebhookClient.Services
{
    /// <summary>
    /// Request payload used by the WebhookClient to register a subscription with Webhooks.API.
    /// </summary>
    public class WebhookSubscriptionRequest
    {
        /// <summary>
        /// Destination URL receiving webhook POSTs.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Optional shared token sent with the subscription.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Name of the event being subscribed to.
        /// </summary>
        public string? Event { get; set; }

        /// <summary>
        /// Grant URL verified by Webhooks.API before registration.
        /// </summary>
        public string? GrantUrl { get; set; }
    }
}