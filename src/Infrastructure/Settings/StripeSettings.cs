// Infrastructure/Settings/StripeSettings.cs
namespace Infrastructure.Settings;

public sealed class StripeSettings
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = default!;
    public string WebhookSecret { get; set; } = default!;
    public string SuccessUrl { get; set; } = default!; // e.g. https://app/orders/success?session_id={CHECKOUT_SESSION_ID}
    public string CancelUrl { get; set; } = default!;  // e.g. https://app/cart
    public string Currency { get; set; } = "usd";
}