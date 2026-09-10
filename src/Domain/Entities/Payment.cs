// Domain/Entities/Payment.cs
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string StripePaymentIntentId { get; private set; } = string.Empty;
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

    public static Result<Payment> Create(
        Guid orderId,
        string stripePaymentIntentId,
        decimal amount,
        string provider = "Stripe",
        string currency = "USD",
        string? providerMetadata = null)
    {
        var payment = new Payment();

        return payment
            .SetOrderId(orderId)
            .Bind(() => payment.SetProvider(provider))
            .Bind(() => payment.SetStripePaymentIntentId(stripePaymentIntentId))
            .Bind(() => payment.SetAmount(amount))
            .Bind(() => payment.SetCurrency(currency))
            .Bind(() => payment.SetProviderMetadata(providerMetadata))
            .Bind(() => payment.Initialize())
            .Bind(() => Result<Payment>.Success(payment));
    }

    public Result SetStripeReference(string sessionOrIntentId, string? paymentIntentId = null)
    {
        if (string.IsNullOrWhiteSpace(sessionOrIntentId))
            return Result.Failure(Error.Validation(
                "Payment.StripeReference.Required", "Stripe reference is required."));

        StripePaymentIntentId = string.IsNullOrWhiteSpace(paymentIntentId)
            ? sessionOrIntentId.Trim()
            : paymentIntentId.Trim();

        ProviderMetadata = string.IsNullOrWhiteSpace(paymentIntentId)
            ? ProviderMetadata
            : $"{{\"session_id\":\"{sessionOrIntentId.Trim()}\"}}";

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

    private Result SetStripePaymentIntentId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Result.Failure(Error.Validation(
                "Payment.StripePaymentIntentId.Required", "Stripe payment intent id is required."));
        StripePaymentIntentId = id.Trim();
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

    private Result SetProviderMetadata(string? metadata)
    {
        ProviderMetadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}