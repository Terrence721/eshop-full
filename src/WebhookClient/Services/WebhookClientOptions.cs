namespace eShop.WebhookClient.Services
{
    /// <summary>
    /// Strongly-typed options for WebhookClient configuration.
    /// </summary>
    public class WebhookClientOptions
    {
        /// <summary>
        /// Optional shared-token used for webhook registration/validation.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Public URL of this WebhookClient instance.
        /// </summary>
        public string? SelfUrl { get; set; }

        /// <summary>
        /// Whether incoming webhook tokens must be validated.
        /// </summary>
        public bool ValidateToken { get; set; }
    }
}