// Application/Features/Checkout/Webhooks/StripeWebhookEvents.cs
namespace Application.Checkout.Webhooks;

/// <summary>
/// Normalized webhook payloads. Mapped from Stripe in Infrastructure.
/// Application never sees Stripe types.
/// </summary>
public abstract record StripeWebhookEvent;

public sealed record CheckoutSessionCompletedWebhook(
    string SessionId,
    string? PaymentIntentId,
    string? ClientReferenceId,   // order id you set on the session
    string? CustomerEmail) : StripeWebhookEvent;

public sealed record CheckoutSessionFailedWebhook(
    string SessionId,
    string? PaymentIntentId,
    string? ClientReferenceId,
    string FailureReason) : StripeWebhookEvent;

public sealed record PaymentIntentFailedWebhook(
    string PaymentIntentId,
    string? FailureMessage) : StripeWebhookEvent;

/// <summary>Event type we do not handle — still ACK to Stripe.</summary>
public sealed record UnhandledStripeWebhook(string EventType) : StripeWebhookEvent;