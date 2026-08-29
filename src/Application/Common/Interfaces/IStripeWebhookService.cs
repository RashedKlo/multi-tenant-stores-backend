using Application.Checkout.Webhooks;
using Domain.Common;

namespace Domain.Interfaces;
public interface IStripeWebhookService
{
    /// <summary>
    /// Verifies Stripe-Signature and maps the payload to an application event.
    /// Returns Validation failure when the signature is invalid.
    /// </summary>
    Result<StripeWebhookEvent> ParseAndVerify(string jsonPayload, string stripeSignatureHeader);
}