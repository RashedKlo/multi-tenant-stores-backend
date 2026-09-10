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
        var errors = new List<Error>();
        sessionOrIntentId = DomainValidation.NormalizeRequiredString(sessionOrIntentId, errors, "Stripe reference");

        if (errors.Count > 0)
            return Result.Failure(errors);

        StripePaymentIntentId = string.IsNullOrWhiteSpace(paymentIntentId)
            ? sessionOrIntentId
            : paymentIntentId;

        ProviderMetadata = string.IsNullOrWhiteSpace(paymentIntentId)
            ? null
            : $"{{\"session_id\":\"{sessionOrIntentId}\"}}";

        Touch();
        return Result.Success();
    }

       // Domain/Entities/Payment.cs  (key behavior methods only — keep your Create factory)
public Result MarkSucceeded()
{
    if (Status == PaymentStatus.Succeeded)
        return Result.Success(); // idempotent for webhooks

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
    if (Status == PaymentStatus.Failed)
        return Result.Success();

    if (Status is not PaymentStatus.Pending)
        return Result.Failure(Error.Validation(
            "Payment.Status.Invalid", $"Cannot mark {Status} payment as failed."));

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
        var errors = new List<Error>();
        provider = DomainValidation.NormalizeRequiredString(provider, errors, "Provider");

        if (errors.Count > 0)
            return Result.Failure(errors);

        Provider = provider;
        return Result.Success();
    }

    private Result SetStripePaymentIntentId(string stripePaymentIntentId)
    {
        var errors = new List<Error>();
        stripePaymentIntentId = DomainValidation.NormalizeRequiredString(stripePaymentIntentId, errors, "Stripe payment intent ID");

        if (errors.Count > 0)
            return Result.Failure(errors);

        StripePaymentIntentId = stripePaymentIntentId;
        return Result.Success();
    }

    private Result SetAmount(decimal amount)
    {
        var errors = new List<Error>();
        DomainValidation.EnsureNonNegative(amount, errors, "Amount");

        if (errors.Count > 0)
            return Result.Failure(errors);

        Amount = amount;
        return Result.Success();
    }

    private Result SetCurrency(string currency)
    {
        var errors = new List<Error>();
        currency = DomainValidation.NormalizeRequiredString(currency, errors, "Currency");

        if (errors.Count > 0)
            return Result.Failure(errors);

        Currency = currency.ToUpperInvariant();
        return Result.Success();
    }

    private Result SetProviderMetadata(string? providerMetadata)
    {
        ProviderMetadata = DomainValidation.NormalizeOptional(providerMetadata);
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}