namespace Webhooks.API.Model;

/// <summary>
/// Incoming request payload for creating a webhook subscription.
/// </summary>
public class WebhookSubscriptionRequest : IValidatableObject
{
    // Url/Event/GrantUrl: null! -- RouteHandlerBuilderExtensions.ValidateWebhookSubscriptionRequest()
    // (an endpoint filter chained onto the MapPost("/") route) runs Validate() below before the
    // handler ever sees this request, rejecting it if any of these three are missing or malformed.
    // The implicit parameterless constructor (used during JSON model binding) leaves them unset,
    // but the external filter guarantees they're valid by the time a handler actually uses them.
    /// <summary>
    /// Destination URL receiving webhook POSTs.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Optional shared token echoed to subscribers.
    /// </summary>
    // Genuinely optional: Validate() below never checks Token, matching
    // WebhookSubscription.Token's own established nullability.
    public string? Token { get; set; }

    /// <summary>
    /// Name of the subscribed event (matches <see cref="WebhookType" />).
    /// </summary>
    public string Event { get; set; } = null!;

    /// <summary>
    /// Grant URL checked by the tester before subscription creation.
    /// </summary>
    public string GrantUrl { get; set; } = null!;

    /// <summary>
    /// Validates URL well-formedness and event name.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.IsWellFormedUriString(GrantUrl, UriKind.Absolute))
        {
            yield return new ValidationResult("GrantUrl is not valid", new[] { nameof(GrantUrl) });
        }

        if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
        {
            yield return new ValidationResult("Url is not valid", new[] { nameof(Url) });
        }

        var isOk = Enum.TryParse(Event, ignoreCase: true, result: out WebhookType whtype);
        if (!isOk)
        {
            yield return new ValidationResult($"{Event} is invalid event name", new[] { nameof(Event) });
        }
    }
}