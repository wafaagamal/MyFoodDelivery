namespace OrderingSvc.Infrastructure.Services;

/// <summary>
/// Stripe configuration settings. Bound from appsettings.json "Stripe" section.
/// </summary>
public class StripeSettings
{
    public string SecretKey { get; set; } = default!;
    public string PublishableKey { get; set; } = default!;
    public string WebhookSecret { get; set; } = default!;
}
