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
    IOrderStatusHistoryRepository statusHistoryRepository,
    ICartRepository cartRepository,
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

            UnhandledStripeWebhook
                => Result.Success(),

            _ => Result.Success()
        };
    }

    private async Task<Result> OnCheckoutSucceededAsync(
        CheckoutSessionCompletedWebhook e,
        CancellationToken ct)
    {
        var payment = await FindPaymentAsync(
            e.SessionId, e.PaymentIntentId, e.ClientReferenceId, ct);

        if (payment is null)
        {
            logger.LogWarning(
                "checkout.session.completed: payment not found. Session={SessionId}, ClientRef={ClientRef}",
                e.SessionId, e.ClientReferenceId);
            return Result.Success();
        }

        if (payment.Status == PaymentStatus.Succeeded)
            return Result.Success(); // idempotent

        if (!string.IsNullOrWhiteSpace(e.PaymentIntentId))
            payment.SetStripeReference(e.SessionId, e.PaymentIntentId);

        payment.MarkSucceeded();
        paymentRepository.Update(payment);

        var order = await orderRepository.GetByIdAsync(payment.OrderId, ct);
        if (order is not null && order.Status == OrderStatus.Pending)
        {
            order.ChangeStatus(OrderStatus.Confirmed);
            orderRepository.Update(order);

            var history = OrderStatusHistory.Create(
                order.Id,
                OrderStatus.Confirmed,
                note: "Payment succeeded (Stripe Checkout)",
                changedByType: ChangedByType.System);

            if (history.IsSuccess)
                await statusHistoryRepository.AddAsync(history.Value!, ct);

            try
            {
                await cartRepository.ClearForCustomerStoreAsync(
                    order.CustomerId, order.StoreId, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clear cart after order {OrderId}", order.Id);
            }
        }

        await orderRepository.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> OnCheckoutFailedAsync(
        CheckoutSessionFailedWebhook e,
        CancellationToken ct)
    {
        var payment = await FindPaymentAsync(
            e.SessionId, e.PaymentIntentId, e.ClientReferenceId, ct);

        if (payment is null || payment.Status == PaymentStatus.Succeeded)
            return Result.Success();

        payment.MarkFailed(e.FailureReason);
        paymentRepository.Update(payment);

        var order = await orderRepository.GetByIdAsync(payment.OrderId, ct);
        if (order is not null && order.Status == OrderStatus.Pending)
        {
            order.ChangeStatus(OrderStatus.Cancelled);
            orderRepository.Update(order);

            var history = OrderStatusHistory.Create(
                order.Id,
                OrderStatus.Cancelled,
                note: $"Checkout ended without payment ({e.FailureReason})",
                changedByType: ChangedByType.System);

            if (history.IsSuccess)
                await statusHistoryRepository.AddAsync(history.Value!, ct);
        }

        await orderRepository.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result> OnPaymentIntentFailedAsync(
        PaymentIntentFailedWebhook e,
        CancellationToken ct)
    {
        var payment = await paymentRepository
            .GetByStripePaymentIntentIdAsync(e.PaymentIntentId, ct);

        if (payment is null || payment.Status == PaymentStatus.Succeeded)
            return Result.Success();

        payment.MarkFailed(e.FailureMessage);
        paymentRepository.Update(payment);
        await paymentRepository.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Payment?> FindPaymentAsync(
        string sessionId,
        string? paymentIntentId,
        string? clientReferenceId,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(paymentIntentId))
        {
            var byPi = await paymentRepository
                .GetByStripePaymentIntentIdAsync(paymentIntentId, ct);
            if (byPi is not null)
                return byPi;
        }

        var bySession = await paymentRepository
            .GetByStripePaymentIntentIdAsync(sessionId, ct);
        if (bySession is not null)
            return bySession;

        if (Guid.TryParse(clientReferenceId, out var orderId))
            return await paymentRepository.GetByOrderIdAsync(orderId, ct);

        return null;
    }
}