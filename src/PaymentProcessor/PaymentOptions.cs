namespace eShop.PaymentProcessor;

/// <summary>
/// Configuration for the payment processor, used to simulate payment outcomes.
/// </summary>
public class PaymentOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether payment is simulated as successful.
    /// </summary>
    public bool PaymentSucceeded { get; set; }
}
