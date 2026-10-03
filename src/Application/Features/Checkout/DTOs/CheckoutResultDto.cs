
namespace Application.Checkout.DTOs;
public sealed record CreateCheckoutSessionRequest(
    Guid OrderId,
    Guid CustomerId,
    Guid StoreId,
    decimal Amount,
    string Currency,
    string SuccessUrl,
    string CancelUrl,
    string? Description = null,
    string? CustomerEmail = null,
    string? IdempotencyKey = null);

public sealed record CreateCheckoutSessionResult(
    string SessionId,
    string CheckoutUrl,
    string? PaymentIntentId); // may be null until session is completed
    public sealed record CheckoutResultDto(
    Guid OrderId,
    string CheckoutUrl,
    string SessionId);