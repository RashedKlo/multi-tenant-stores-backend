// Domain/Entities/Payment.cs
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = string.Empty;

    /// <summary>Stripe Checkout Session id (cs_...). Set right after the session is created.</summary>
    public string? StripeSessionId { get; private set; }

    /// <summary>Stripe PaymentIntent id (pi_...). Known once Stripe reports it; may stay null for a while.</summary>
    public string? StripePaymentIntentId { get; private set; }

    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? FailureReason { get; private set; }
    public string? ProviderMetadata { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Order Order { get; private set; } = null!;

    private Payment() { }

    /// <summary>Creates a Pending payment BEFORE the provider session exists.</summary>
    public static Result<Payment> Initiate(
        Guid orderId,
        decimal amount,
        string provider = "Stripe",
        string currency = "USD")
    {
        var payment = new Payment();

        return payment
            .SetOrderId(orderId)
            .Bind(() => payment.SetProvider(provider))
            .Bind(() => payment.SetAmount(amount))
            .Bind(() => payment.SetCurrency(currency))
            .Bind(() => payment.Initialize())
            .Bind(() => Result<Payment>.Success(payment));
    }

    /// <summary>
    /// Stores the Stripe ids. Safe to call repeatedly (checkout, then webhook):
    /// the payment intent id is only overwritten when a non-empty value is supplied.
    /// </summary>
    public Result AttachCheckoutSession(string sessionId, string? paymentIntentId = null)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return Result.Failure(Error.Validation(
                "Payment.StripeSessionId.Required", "Stripe session id is required."));

        StripeSessionId = sessionId.Trim();

        if (!string.IsNullOrWhiteSpace(paymentIntentId))
            StripePaymentIntentId = paymentIntentId.Trim();

        Touch();
        return Result.Success();
    }

    public Result MarkSucceeded()
    {
        if (Status == PaymentStatus.Succeeded)
            return Result.Success();

        if (Status is not PaymentStatus.Pending)
            return Result.Failure(Error.Validation(
                "Payment.Status.Invalid", $"Cannot mark {Status} payment as succeeded."));

        Status = PaymentStatus.Succeeded;
        PaidAt = DateTime.UtcNow;
        Touch();
        return Result.Success();
    }

    public Result MarkFailed(string? failureReason = null)
    {
        if (Status is PaymentStatus.Succeeded or PaymentStatus.Refunded)
            return Result.Failure(Error.Validation(
                "Payment.Status.Invalid", $"Cannot mark {Status} payment as failed."));

        if (Status == PaymentStatus.Failed)
            return Result.Success();

        Status = PaymentStatus.Failed;
        FailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim();
        Touch();
        return Result.Success();
    }

    public Result MarkRefunded()
    {
        if (Status == PaymentStatus.Refunded)
            return Result.Success();

        if (Status is not PaymentStatus.Succeeded)
            return Result.Failure(Error.Validation(
                "Payment.Status.Invalid", "Only succeeded payments can be refunded."));

        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        Touch();
        return Result.Success();
    }

    private Result Initialize()
    {
        Id = Guid.NewGuid();
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        return Result.Success();
    }

    private Result SetOrderId(Guid orderId)
    {
        if (orderId == Guid.Empty)
            return Result.Failure(Error.Validation("Payment.OrderId.Required", "OrderId is required."));
        OrderId = orderId;
        return Result.Success();
    }

    private Result SetProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
            return Result.Failure(Error.Validation("Payment.Provider.Required", "Provider is required."));
        Provider = provider.Trim();
        return Result.Success();
    }

    private Result SetAmount(decimal amount)
    {
        if (amount < 0)
            return Result.Failure(Error.Validation("Payment.Amount.Invalid", "Amount cannot be negative."));
        Amount = amount;
        return Result.Success();
    }

    private Result SetCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure(Error.Validation("Payment.Currency.Required", "Currency is required."));
        Currency = currency.Trim().ToUpperInvariant();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}