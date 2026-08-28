// Application/Features/Checkout/Commands/Checkout/CheckoutHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Application.Checkout.DTOs; 
namespace Application.Features.Checkout.Commands.Checkout;

public sealed class CheckoutHandler(
    ICurrentUserService currentUser,
    ICartQueries cartQueries,
    ICustomerAddressRepository addressRepository,
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    IOrderItemRepository orderItemRepository,
    IOrderItemOptionRepository orderItemOptionRepository,
    IOrderStatusHistoryRepository statusHistoryRepository,
    IPaymentRepository paymentRepository,
    IPaymentService paymentService) : IRequestHandler<CheckoutCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(
        CheckoutCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Auth ──────────────────────────────────────────────
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<CheckoutResultDto>.Failure(
                Error.Unauthorized("Checkout.Unauthorized", "Customer must be authenticated."));

        var customerId = currentUser.CustomerId.Value;

        // ── 2. Address (ownership enforced in query) ─────────────
        var address = await addressRepository.GetByIdForCustomerAsync(
            request.AddressId, customerId, cancellationToken);

        if (address is null || address.IsDeleted)
            return Result<CheckoutResultDto>.Failure(
                Error.NotFound("Checkout.AddressNotFound", "Delivery address not found."));

        // ── 3. Customer (name + email for Stripe) ────────────────
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return Result<CheckoutResultDto>.Failure(
                Error.NotFound("Checkout.CustomerNotFound", "Customer not found."));

        var deliveryName = $"{customer.FirstName} {customer.LastName}".Trim();
        var deliveryPhone = string.IsNullOrWhiteSpace(request.DeliveryPhone)
            ? null
            : request.DeliveryPhone.Trim();

        // ── 4. Cart (single high-performance query) ──────────────
        var cart = await cartQueries.GetCartForCheckoutAsync(
            customerId, request.StoreId, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            return Result<CheckoutResultDto>.Failure(
                Error.Validation("Checkout.EmptyCart", "Cart is empty for this store."));

        // ── 5. Validate availability ─────────────────────────────
        var validationErrors = new List<Error>();

        foreach (var item in cart.Items)
        {
            if (!item.IsAvailable)
            {
                validationErrors.Add(Error.Validation(
                    "Checkout.ProductUnavailable",
                    $"Product '{item.NameEn}' is unavailable or out of stock."));
                continue;
            }

            foreach (var opt in item.Options.Where(o => !o.IsAvailable))
            {
                validationErrors.Add(Error.Validation(
                    "Checkout.OptionUnavailable",
                    $"Option '{opt.NameEn}' on '{item.NameEn}' is no longer available."));
            }
        }

        if (validationErrors.Count > 0)
            return Result<CheckoutResultDto>.Failure(validationErrors);

        // ── 6. Totals (server-side only) ─────────────────────────
        var subtotal = cart.Items.Sum(i => i.LineTotal);
        const decimal discountTotal = 0m; // wire discounts later
        var total = subtotal - discountTotal;

        if (total <= 0)
            return Result<CheckoutResultDto>.Failure(
                Error.Validation("Checkout.InvalidTotal", "Order total must be greater than zero."));

        // ── 7. Domain aggregate ──────────────────────────────────
        var orderResult = Order.Create(
            customerId: customerId,
            storeId: request.StoreId,
            deliveryName: deliveryName,
            deliveryAddressText: address.AddressText,
            deliveryLatitude: address.Latitude,
            deliveryLongitude: address.Longitude,
            subtotal: subtotal,
            discountTotal: discountTotal,
            addressId: address.Id,
            deliveryPhone: deliveryPhone);

        if (orderResult.IsFailure)
            return Result<CheckoutResultDto>.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        var orderItems = new List<OrderItem>(cart.Items.Count);
        var orderItemOptions = new List<OrderItemOption>();

        foreach (var cartItem in cart.Items)
        {
            // unit price already includes option adjustments (matches OrderItem.Create math)
            var itemResult = OrderItem.Create(
                orderId: order.Id,
                nameEnSnapshot: cartItem.NameEn,
                nameArSnapshot: cartItem.NameAr,
                unitPriceSnapshot: cartItem.EffectiveUnitPrice,
                quantity: cartItem.Quantity,
                productId: cartItem.ProductId);

            if (itemResult.IsFailure)
                return Result<CheckoutResultDto>.Failure(itemResult.Errors);

            var orderItem = itemResult.Value!;
            orderItems.Add(orderItem);

            foreach (var opt in cartItem.Options)
            {
                var optResult = OrderItemOption.Create(
                    orderItemId: orderItem.Id,
                    optionNameEnSnapshot: opt.NameEn,
                    optionNameArSnapshot: opt.NameAr,
                    priceAdjustmentSnapshot: opt.PriceAdjustment);

                if (optResult.IsFailure)
                    return Result<CheckoutResultDto>.Failure(optResult.Errors);

                orderItemOptions.Add(optResult.Value!);
            }
        }

        var historyResult = OrderStatusHistory.Create(
            orderId: order.Id,
            status: OrderStatus.Pending,
            note: "Order created — awaiting payment",
            changedByType: ChangedByType.Customer,
            changedById: customerId);

        if (historyResult.IsFailure)
            return Result<CheckoutResultDto>.Failure(historyResult.Errors);

        // Placeholder Stripe id — replaced with real SessionId after Stripe call
        var placeholderIntentId = $"pending_{order.Id:N}";

        var paymentResult = Payment.Create(
            orderId: order.Id,
            stripePaymentIntentId: placeholderIntentId,
            amount: total,
            provider: "Stripe",
            currency: "USD");

        if (paymentResult.IsFailure)
            return Result<CheckoutResultDto>.Failure(paymentResult.Errors);

        var payment = paymentResult.Value!;

        // ── 8. Persist (one SaveChanges — shared scoped DbContext) ─
        orderRepository.Add(order);
        await orderItemRepository.AddRangeAsync(orderItems, cancellationToken);
        await orderItemOptionRepository.AddRangeAsync(orderItemOptions, cancellationToken);
        await statusHistoryRepository.AddAsync(historyResult.Value!, cancellationToken);
        paymentRepository.Add(payment);

        await orderRepository.SaveChangesAsync(cancellationToken);

        // ── 9. Stripe Checkout Session ───────────────────────────
        CreateCheckoutSessionResult session;
        try
        {
            session = await paymentService.CreateCheckoutSessionAsync(
                new CreateCheckoutSessionRequest(
                    OrderId: order.Id,
                    CustomerId: customerId,
                    StoreId: request.StoreId,
                    Amount: total,
                    Currency: "usd",
                    SuccessUrl: string.Empty, // service falls back to StripeSettings
                    CancelUrl: string.Empty,
                    CustomerEmail: customer.Email,
                    Description: $"Order {order.Id:N}"),
                cancellationToken);
        }
        catch (Exception)
        {
            // Best-effort: mark order cancelled so it does not stay Pending forever
            order.ChangeStatus(OrderStatus.Cancelled);
            orderRepository.Update(order);
            await orderRepository.SaveChangesAsync(cancellationToken);

            return Result<CheckoutResultDto>.Failure(
                Error.Failure("Checkout.PaymentProviderError",
                    "Could not start payment session. Please try again."));
        }

        // Store SessionId in the column used by webhooks for lookup.
        // (Webhook Phase 2 will also receive PaymentIntent id.)
        // Payment has no public setter — update via a small domain method or re-create.
        // Minimal approach: add a domain method on Payment (see note below).
        payment.SetStripeReference(session.SessionId, session.PaymentIntentId);
        paymentRepository.Update(payment);
        await paymentRepository.SaveChangesAsync(cancellationToken);

        return Result<CheckoutResultDto>.Success(
            new CheckoutResultDto(order.Id, session.CheckoutUrl, session.SessionId));
    }
}