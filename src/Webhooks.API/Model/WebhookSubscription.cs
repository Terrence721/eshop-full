namespace Webhooks.API.Model;

public class WebhookSubscription
{
    public int Id { get; set; }

    public WebhookType Type { get; set; }
    public DateTime Date { get; set; }
    // [Required] -- always set at the one real construction site (WebHooksApi.cs) from
    // request.Url/user.GetUserId(); the implicit parameterless constructor never runs in
    // practice.
    [Required]
    public string DestUrl { get; set; } = null!;
    // Genuinely optional, confirmed by real usage: WebHooksApi.cs assigns it directly from
    // the nullable request.Token, and WebhooksSender.cs guards it with
    // string.IsNullOrWhiteSpace before use.
    public string? Token { get; set; }
    [Required]
    public string UserId { get; set; } = null!;
}
