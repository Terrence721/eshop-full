namespace Webhooks.API.Exceptions;

/// <summary>
/// Exception type for domain (webhooks) errors.
/// </summary>
public class WebhooksDomainException : Exception
{
    public WebhooksDomainException()
    { }

    public WebhooksDomainException(string message)
        : base(message)
    { }

    public WebhooksDomainException(string message, Exception innerException)
        : base(message, innerException)
    { }
}