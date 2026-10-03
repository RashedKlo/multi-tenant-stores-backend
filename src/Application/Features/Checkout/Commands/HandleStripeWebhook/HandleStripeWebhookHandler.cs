// Application/Features/Checkout/Commands/HandleStripeWebhook/HandleStripeWebhookHandler.cs
using Application.Checkout.Webhooks;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Checkout.Commands.HandleStripeWebhook;

public sealed class HandleStripeWebhookHandler(
    IStripeWebhookService stripeWebhook,
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    ICouponRepository couponRepository,
    IStockRepository stockRepository,
    IUnitOfWork unitOfWork,
    ILogger<HandleStripeWebhookHandler> logger)
    : IRequestHandler<HandleStripeWebhookCommand, Result>
{
    public async Task<Result> Handle(
        HandleStripeWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var parsed = stripeWebhook.ParseAndVerify(
            request.JsonPayload,
            request.StripeSignatureHeader);

        if (parsed.IsFailure)
            return parsed;

        return parsed.Value switch
        {
            CheckoutSessionCompletedWebhook e
                => await OnCheckoutSucceededAsync(e, cancellationToken),
            CheckoutSessionFailedWebhook e
                => await OnCheckoutFailedAsync(e, cancellationToken),
            PaymentIntentFailedWebhook e
                => await OnPaymentIntentFailedAsync(e, cancellationToken),
            _ => Result.Success()
        };
    }

    private async Task<Result> OnCheckoutSucceededAsync(
        CheckoutSessionCompletedWebhook e, CancellationToken ct)
    {
        var payment = await FindPaymentAsync(e.SessionId, e.PaymentIntentId, e.ClientReferenceId, ct);
        if (payment is null)
        {
            logger.LogWarning(
                "checkout.session.completed: payment not found. Session={SessionId}",
                e.SessionId);
            return Result.Success();
        }

        if (!string.IsNullOrWhiteSpace(e.PaymentIntentId))
          payment.AttachCheckoutSession(e.SessionId, e.PaymentIntentId);

        var payResult = payment.MarkSucceeded();

        if (payResult.IsFailure)
            return payResult;


        var order = await orderRepository.GetByIdAsync(payment.OrderId, ct);
        if (order is not null)
        {
            var confirm = order.ConfirmPaymentReceived();
            if (confirm.IsFailure && order.Status != OrderStatus.Confirmed)
                return confirm;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> OnCheckoutFailedAsync(
    CheckoutSessionFailedWebhook e, CancellationToken ct)
{
    var payment = await FindPaymentAsync(e.SessionId, e.PaymentIntentId, e.ClientReferenceId, ct);
    if (payment is null || payment.Status == PaymentStatus.Succeeded)
        return Result.Success();

    return await unitOfWork.ExecuteInTransactionAsync(async token =>
    {
        payment.MarkFailed(e.FailureReason);

        var order = await orderRepository.GetByIdAsync(payment.OrderId, token);
        if (order is not null && order.Status == OrderStatus.Pending)
        {
            order.Cancel($"Checkout ended without payment ({e.FailureReason})", ChangedByType.System);

            await stockRepository.ReleaseForOrderAsync(order.Id, token);
            if (order.CouponId is { } couponId)
                await couponRepository.ReleaseRedemptionAsync(couponId, token);
        }

        await unitOfWork.SaveChangesAsync(token);
        return Result.Success();
    }, ct);
}
    private async Task<Result> OnPaymentIntentFailedAsync(
        PaymentIntentFailedWebhook e, CancellationToken ct)
    {
        var payment = await paymentRepository.GetByStripePaymentIntentIdAsync(e.PaymentIntentId, ct);
        if (payment is null || payment.Status == PaymentStatus.Succeeded)
            return Result.Success();

        payment.MarkFailed(e.FailureMessage);
        await paymentRepository.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Payment?> FindPaymentAsync(
        string sessionId, string? paymentIntentId, string? clientReferenceId, CancellationToken ct)
    {
         var bySession = await paymentRepository.GetByStripeSessionIdAsync(sessionId, ct);
        if (bySession is not null) return bySession;

        return null;
    }
}