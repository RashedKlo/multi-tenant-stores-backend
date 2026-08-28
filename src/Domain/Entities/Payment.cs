using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }

        public Guid OrderId { get; private set; }

        public string Provider { get; private set; } = null!;

        public string StripePaymentIntentId { get; private set; } = null!;

        public PaymentStatus Status { get; private set; }

        public decimal Amount { get; private set; }

        public string Currency { get; private set; } = null!;

        public string? FailureReason { get; private set; }

        public string? ProviderMetadata { get; private set; }   // jsonb as string

        public DateTime? PaidAt { get; private set; }

        public DateTime? RefundedAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }
        public Order Order { get; private set; } = null!;

        private Payment()
        {
        }

        public static Result<Payment> Create(
            Guid orderId,
            string stripePaymentIntentId,
            decimal amount,
            string provider = "Stripe",
            string currency = "USD",
            string? providerMetadata = null)
        {
            var errors = new List<Error>();

            DomainValidation.EnsureNotEmptyGuid(orderId, errors, "OrderId");

            stripePaymentIntentId = DomainValidation.NormalizeRequiredString(stripePaymentIntentId, errors, "Stripe payment intent ID");
            provider = DomainValidation.NormalizeRequiredString(provider, errors, "Provider");
            currency = DomainValidation.NormalizeRequiredString(currency, errors, "Currency");

            DomainValidation.EnsureNonNegative(amount, errors, "Amount");

            providerMetadata = DomainValidation.NormalizeOptional(providerMetadata);

            if (errors.Count > 0)
                return Result<Payment>.Failure(errors);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Provider = provider,
                StripePaymentIntentId = stripePaymentIntentId,
                Status = PaymentStatus.Pending,
                Amount = amount,
                Currency = currency.ToUpperInvariant(),
                ProviderMetadata = providerMetadata,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Result<Payment>.Success(payment);
        }
// Domain/Entities/Payment.cs — ADD
public Result SetStripeReference(string sessionOrIntentId, string? paymentIntentId = null)
{
    var errors = new List<Error>();
    sessionOrIntentId = DomainValidation.NormalizeRequiredString(
        sessionOrIntentId, errors, "Stripe reference");

    if (errors.Count > 0)
        return Result.Failure(errors);

    // Prefer real PaymentIntent when available; otherwise keep Session id
    StripePaymentIntentId = string.IsNullOrWhiteSpace(paymentIntentId)
        ? sessionOrIntentId
        : paymentIntentId;

    ProviderMetadata = string.IsNullOrWhiteSpace(paymentIntentId)
        ? null
        : $"{{\"session_id\":\"{sessionOrIntentId}\"}}";

    UpdatedAt = DateTime.UtcNow;
    return Result.Success();
}
        public Result MarkSucceeded()
        {
            Status = PaymentStatus.Succeeded;
            PaidAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkFailed(string? failureReason = null)
        {
            Status = PaymentStatus.Failed;
            FailureReason = DomainValidation.NormalizeOptional(failureReason);
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public Result MarkRefunded()
        {
            Status = PaymentStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }
    }
}