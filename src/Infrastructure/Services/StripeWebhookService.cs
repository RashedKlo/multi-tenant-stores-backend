// Infrastructure/Services/StripeWebhookService.cs
using Application.Checkout.Webhooks;

using Domain.Common;
using Domain.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services;

public sealed class StripeWebhookService(IOptions<StripeSettings> settings) :IStripeWebhookService
{
    private readonly string _webhookSecret = settings.Value.WebhookSecret;

    public Result<StripeWebhookEvent> ParseAndVerify(
        string jsonPayload,
        string stripeSignatureHeader)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                jsonPayload,
                stripeSignatureHeader,
                _webhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            return Result<StripeWebhookEvent>.Failure(
                Error.Validation("Stripe.InvalidSignature", ex.Message));
        }

        return Result<StripeWebhookEvent>.Success(Map(stripeEvent));
    }

private static StripeWebhookEvent Map(Event stripeEvent) =>
    stripeEvent.Type switch
    {
        "checkout.session.completed"
            or "checkout.session.async_payment_succeeded"
            => MapCompleted(stripeEvent),

        "checkout.session.expired"
            or "checkout.session.async_payment_failed"
            => MapFailed(stripeEvent, stripeEvent.Type),

        "payment_intent.payment_failed"
            => MapPaymentIntentFailed(stripeEvent),

        _ => new UnhandledStripeWebhook(stripeEvent.Type)
    };

    private static StripeWebhookEvent MapCompleted(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not Session session)
            return new UnhandledStripeWebhook(stripeEvent.Type);

        return new CheckoutSessionCompletedWebhook(
            SessionId: session.Id,
            PaymentIntentId: session.PaymentIntentId,
            ClientReferenceId: session.ClientReferenceId,
            CustomerEmail: session.CustomerEmail);
    }

    private static StripeWebhookEvent MapFailed(Event stripeEvent, string reason)
    {
        if (stripeEvent.Data.Object is not Session session)
            return new UnhandledStripeWebhook(stripeEvent.Type);

        return new CheckoutSessionFailedWebhook(
            SessionId: session.Id,
            PaymentIntentId: session.PaymentIntentId,
            ClientReferenceId: session.ClientReferenceId,
            FailureReason: reason);
    }

    private static StripeWebhookEvent MapPaymentIntentFailed(Event stripeEvent)
    {
        if (stripeEvent.Data.Object is not PaymentIntent intent)
            return new UnhandledStripeWebhook(stripeEvent.Type);

        return new PaymentIntentFailedWebhook(
            PaymentIntentId: intent.Id,
            FailureMessage: intent.LastPaymentError?.Message);
    }
}