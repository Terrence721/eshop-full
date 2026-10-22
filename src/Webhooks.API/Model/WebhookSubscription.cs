namespace Webhooks.API.Model
{
    /// <summary>
    /// A persisted webhook subscription.
    /// </summary>
    public class WebhookSubscription
    {
        /// <summary>
        /// Database primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kind of webhook event subscribed to.
        /// </summary>
        public WebhookType Type { get; set; }

        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Destination URL receiving webhook POSTs.
        /// </summary>
        // [Required] -- always set at the one real construction site (WebHooksApi.cs) from
        // request.Url/user.GetUserId(); the implicit parameterless constructor never runs in
        // practice.
        [Required]
        public string DestUrl { get; set; } = null!;

        /// <summary>
        /// Optional shared token echoed in the <c>X-eshop-whtoken</c> header.
        /// </summary>
        // Genuinely optional, confirmed by real usage: WebHooksApi.cs assigns it directly from
        // the nullable request.Token, and WebhooksSender.cs guards it with
        // string.IsNullOrWhiteSpace before use.
        public string? Token { get; set; }

        /// <summary>
        /// Owning user's subject id.
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;
    }
}