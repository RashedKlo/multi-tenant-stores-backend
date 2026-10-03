// Application/Features/Checkout/Commands/Checkout/CheckoutHandler.cs
using Application.Checkout.DTOs;
using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Checkout.Commands.Checkout;

/// <summary>
/// Checkout flow
///   1. Read + validate           (address, customer, cart, availability, coupon)
///   2. Price the order           (CheckoutPricingService: item discounts, then coupon)
///   3. TX 1                      reserve stock, redeem coupon, insert Order(Pending) + Payment(Pending)
///   4. Stripe                    create the checkout session (outside any DB transaction, idempotent)
///   5. TX 2                      attach the Stripe session to the payment and clear the cart
///   Stripe failure -> TX 3       cancel order, fail payment, release stock + coupon.
///   Later failures (session expired / async payment failed) are released by the Stripe webhook.
/// </summary>
public sealed class CheckoutHandler(
    ICurrentUserService currentUser,
    ICartQueries cartQueries,
    ICartRepository cartRepository,
    ICustomerAddressRepository addressRepository,
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    ICouponRepository couponRepository,
    IStockRepository stockRepository,
    IPaymentService paymentService,
    IUnitOfWork unitOfWork,
    ILogger<CheckoutHandler> logger)
    : IRequestHandler<CheckoutCommand, Result<CheckoutResultDto>>
{
    private const string Currency = "USD";
    private const string PaymentProvider = "Stripe";

    public async Task<Result<CheckoutResultDto>> Handle(
        CheckoutCommand request,
        CancellationToken cancellationToken)
    {
        // ── 1. Authentication ────────────────────────────────────────────
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is not { } customerId)
            return Fail(Error.Unauthorized("Checkout.Unauthorized", "Customer must be authenticated."));

        // ── 2. Load + validate ───────────────────────────────────────────
        var address = await addressRepository.GetByIdForCustomerAsync(
            request.AddressId, customerId, cancellationToken);

        if (address is null || address.IsDeleted)
            return Fail(Error.NotFound("Checkout.AddressNotFound", "Delivery address not found."));

        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null || customer.IsDeleted)
            return Fail(Error.NotFound("Checkout.CustomerNotFound", "Customer not found."));

        var cart = await cartQueries.GetCartForCheckoutAsync(
            customerId, request.StoreId, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            return Fail(Error.Validation("Checkout.EmptyCart", "Cart is empty for this store."));

        var availabilityErrors = ValidateAvailability(cart.Items);
        if (availabilityErrors.Count > 0)
            return Fail(availabilityErrors);

        var now = DateTime.UtcNow;

        Coupon? coupon = null;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            coupon = await couponRepository.GetActiveByStoreAndCodeAsync(
                request.StoreId, request.CouponCode.Trim(), now, cancellationToken);

            if (coupon is null)
                return Fail(Error.NotFound("Coupon.NotFound", "Coupon was not found or is no longer active."));
        }

        // ── 3. Pricing ───────────────────────────────────────────────────
        var pricingResult = CheckoutPricingService.Calculate(ToPricedLines(cart.Items), coupon, now);
        if (pricingResult.IsFailure)
            return Fail(pricingResult.Errors);

        var pricing = pricingResult.Value!;

        // ── 4. Build aggregates ──────────────────────────────────────────
        var orderResult = Order.Create(
            customerId,
            request.StoreId,
            BuildDeliveryName(customer),
            address.AddressText,
            address.Latitude,
            address.Longitude,
            ToOrderLines(cart.Items),
            address.Id,
            coupon?.Id,
            NormalizePhone(request.DeliveryPhone),
            discountTotal: pricing.DiscountTotal); // item discounts + coupon

        if (orderResult.IsFailure)
            return Fail(orderResult.Errors);

        var order = orderResult.Value!;

        if (order.Total <= 0)
            return Fail(Error.Validation("Checkout.InvalidTotal", "Order total must be greater than zero."));


        var paymentResult = Payment.Initiate(order.Id, order.Total, PaymentProvider, Currency);
        if (paymentResult.IsFailure)
            return Fail(paymentResult.Errors);

        var payment = paymentResult.Value!;

        // ── 5. TX 1: reserve stock, redeem coupon, persist order + payment ─
        var stockRequests = ToStockRequests(cart.Items);

        var persisted = await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var stock = await stockRepository.TryReserveAsync(stockRequests, token);
            if (stock.IsFailure)
                return stock;

            if (coupon is not null && !await couponRepository.TryRedeemAsync(coupon.Id, now, token))
                return Result.Failure(
                    Error.Conflict("Coupon.Exhausted", "Coupon has reached its usage limit."));

            await orderRepository.AddAsync(order, token);
            await paymentRepository.AddAsync(payment, token);
            await unitOfWork.SaveChangesAsync(token);

            return Result.Success();
        }, cancellationToken);

        if (persisted.IsFailure)
            return Fail(persisted.Errors);

        // ── 6. Stripe (outside the DB transaction) ───────────────────────
        CreateCheckoutSessionResult session;
        try
        {
            session = await paymentService.CreateCheckoutSessionAsync(
                new CreateCheckoutSessionRequest(
                    OrderId: order.Id,
                    CustomerId: customerId,
                    StoreId: request.StoreId,
                    Amount: order.Total,
                    Currency: Currency.ToLowerInvariant(),
                    SuccessUrl: string.Empty, // StripePaymentService falls back to StripeSettings
                    CancelUrl: string.Empty,
                    Description: $"Order {order.Id:N}",
                    CustomerEmail: customer.Email,
                    IdempotencyKey: order.Id.ToString("N")),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Stripe session creation failed for order {OrderId}", order.Id);
            await CompensateAsync(order, payment, "Payment session creation failed");

            return Fail(Error.Failure(
                "Checkout.PaymentProviderError",
                "Could not start payment session. Please try again."));
        }

        // ── 7. TX 2: link the session to the payment, then clear the cart ─
        var attached = payment.AttachCheckoutSession(session.SessionId, session.PaymentIntentId);
        if (attached.IsFailure)
        {
            logger.LogError("Stripe returned an unusable session for order {OrderId}: {@Errors}",
                order.Id, attached.Errors);
            await CompensateAsync(order, payment, "Payment session was invalid");

            return Fail(Error.Failure(
                "Checkout.PaymentProviderError",
                "Could not start payment session. Please try again."));
        }

        var domainCart = await cartRepository.GetByCustomerAndStoreAsync(
            customerId, request.StoreId, cancellationToken);
        domainCart?.Clear();

        // One SaveChanges => payment update + cart clear are atomic.
        // If this throws, the webhook still finds the payment via ClientReferenceId (order id)
        // and the Stripe session expiry releases the reservation.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CheckoutResultDto>.Success(
            new CheckoutResultDto(order.Id, session.CheckoutUrl, session.SessionId));
    }

    // ════════════════════════════════════════════════════════════════════
    //  COMPENSATION (TX 3)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Undoes TX 1 after a downstream failure. Never throws: the caller must see the original error.
    /// Stock is released from the persisted order items; the coupon from order.CouponId.
    /// </summary>
    private async Task CompensateAsync(Order order, Payment payment, string reason)
    {
        try
        {
            await unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                order.Cancel(reason, ChangedByType.System);
                payment.MarkFailed(reason);

                await stockRepository.ReleaseForOrderAsync(order.Id, token);

                if (order.CouponId is { } couponId)
                    await couponRepository.ReleaseRedemptionAsync(couponId, token);

                await unitOfWork.SaveChangesAsync(token);
                return Result.Success();
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex, "Compensation failed for order {OrderId}; manual cleanup required", order.Id);
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  VALIDATION
    // ════════════════════════════════════════════════════════════════════

    private static List<Error> ValidateAvailability(IReadOnlyList<CheckoutCartItemDto> items)
    {
        var errors = new List<Error>();

        foreach (var item in items)
        {
            if (!item.IsAvailable)
            {
                errors.Add(Error.Validation(
                    "Checkout.ProductUnavailable",
                    $"Product '{item.NameEn}' is unavailable or out of stock."));
                continue;
            }

            errors.AddRange(item.Options
                .Where(o => !o.IsAvailable)
                .Select(o => Error.Validation(
                    "Checkout.OptionUnavailable",
                    $"Option '{o.NameEn}' on '{item.NameEn}' is no longer available.")));
        }

        return errors;
    }

    // ════════════════════════════════════════════════════════════════════
    //  MAPPING
    // ════════════════════════════════════════════════════════════════════

    private static List<PricedLine> ToPricedLines(IReadOnlyList<CheckoutCartItemDto> items)
        => items
            .Select(i => new PricedLine(i.UnitPrice, i.FinalUnitPrice, i.Quantity, i.OptionsTotal))
            .ToList();

    /// <summary>
    /// Lines carry the BASE unit price; item discounts travel in Order.DiscountTotal
    /// (DB rule: total = subtotal - discount_total).
    /// </summary>
    private static List<OrderLineInput> ToOrderLines(IReadOnlyList<CheckoutCartItemDto> items)
        => items
            .Select(i => new OrderLineInput(
                i.ProductId,
                i.NameEn,
                i.NameAr,
                i.UnitPrice,
                i.Quantity,
                i.Options
                    .Select(o => new OrderOptionInput(o.NameEn, o.NameAr, o.PriceAdjustment))
                    .ToList()))
            .ToList();

    private static List<StockRequest> ToStockRequests(IReadOnlyList<CheckoutCartItemDto> items)
        => items
            .Where(i => i.TrackInventory)
            .GroupBy(i => i.ProductId)
            .Select(g => new StockRequest(g.Key, g.Sum(i => i.Quantity)))
            .ToList();

    private static string BuildDeliveryName(Customer customer)
        => $"{customer.FirstName} {customer.LastName}".Trim();

    private static string? NormalizePhone(string? phone)
        => string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();

    private static Result<CheckoutResultDto> Fail(Error error)
        => Result<CheckoutResultDto>.Failure(error);

    private static Result<CheckoutResultDto> Fail(IReadOnlyList<Error> errors)
        => Result<CheckoutResultDto>.Failure(errors);
}