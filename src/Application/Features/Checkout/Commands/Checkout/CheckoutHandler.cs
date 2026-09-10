// Application/Features/Checkout/Commands/Checkout/CheckoutHandler.cs
using Application.Checkout.DTOs;
using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Checkout.Commands.Checkout;

public sealed class CheckoutHandler(
    ICurrentUserService currentUser,
    ICartQueries cartQueries,
    ICustomerAddressRepository addressRepository,
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    IPaymentRepository paymentRepository,
    IPaymentService paymentService,
    ICartRepository cartRepository)
    : IRequestHandler<CheckoutCommand, Result<CheckoutResultDto>>
{
    public async Task<Result<CheckoutResultDto>> Handle(
        CheckoutCommand request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<CheckoutResultDto>.Failure(
                Error.Unauthorized("Checkout.Unauthorized", "Customer must be authenticated."));

        var customerId = currentUser.CustomerId.Value;

        var address = await addressRepository.GetByIdForCustomerAsync(
            request.AddressId, customerId, cancellationToken);

        if (address is null || address.IsDeleted)
            return Result<CheckoutResultDto>.Failure(
                Error.NotFound("Checkout.AddressNotFound", "Delivery address not found."));

        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null || customer.IsDeleted)
            return Result<CheckoutResultDto>.Failure(
                Error.NotFound("Checkout.CustomerNotFound", "Customer not found."));

        var cart = await cartQueries.GetCartForCheckoutAsync(
            customerId, request.StoreId, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            return Result<CheckoutResultDto>.Failure(
                Error.Validation("Checkout.EmptyCart", "Cart is empty for this store."));

        var availabilityErrors = ValidateAvailability(cart.Items);
        if (availabilityErrors.Count > 0)
            return Result<CheckoutResultDto>.Failure(availabilityErrors);

        var lines = cart.Items.Select(i => new OrderLineInput(
            i.ProductId,
            i.NameEn,
            i.NameAr,
            i.UnitPrice, // base product unit price (options applied in OrderItem.Create)
            i.Quantity,
            i.Options
                .Select(o => new OrderOptionInput(o.NameEn, o.NameAr, o.PriceAdjustment))
                .ToList()
        )).ToList();

        var deliveryName = $"{customer.FirstName} {customer.LastName}".Trim();
        var deliveryPhone = string.IsNullOrWhiteSpace(request.DeliveryPhone)
            ? null
            : request.DeliveryPhone.Trim();

        var orderResult = Order.Create(
            customerId,
            request.StoreId,
            deliveryName,
            address.AddressText,
            address.Latitude,
            address.Longitude,
            lines,
            address.Id,
            deliveryPhone,
            discountTotal: 0m);

        if (orderResult.IsFailure)
            return Result<CheckoutResultDto>.Failure(orderResult.Errors);

        var order = orderResult.Value!;

        if (order.Total <= 0)
            return Result<CheckoutResultDto>.Failure(
                Error.Validation("Checkout.InvalidTotal", "Order total must be greater than zero."));

        orderRepository.Add(order);
        await orderRepository.SaveChangesAsync(cancellationToken);

        CreateCheckoutSessionResult session;
        try
        {
            session = await paymentService.CreateCheckoutSessionAsync(
                new CreateCheckoutSessionRequest(
                    OrderId: order.Id,
                    CustomerId: customerId,
                    StoreId: request.StoreId,
                    Amount: order.Total,
                    Currency: "usd",
                    SuccessUrl: string.Empty,
                    CancelUrl: string.Empty,
                    CustomerEmail: customer.Email,
                    Description: $"Order {order.Id:N}"),
                cancellationToken);
        }
        catch
        {
            order.Cancel("Payment session creation failed", ChangedByType.System);
            await orderRepository.SaveChangesAsync(cancellationToken);

            return Result<CheckoutResultDto>.Failure(
                Error.Failure(
                    "Checkout.PaymentProviderError",
                    "Could not start payment session. Please try again."));
        }

        var stripeRef = string.IsNullOrWhiteSpace(session.PaymentIntentId)
            ? session.SessionId
            : session.PaymentIntentId;

        var paymentResult = Payment.Create(
            order.Id,
            stripeRef,
            order.Total,
            provider: "Stripe",
            currency: "USD",
            providerMetadata: $"{{\"session_id\":\"{session.SessionId}\"}}");

        if (paymentResult.IsFailure)
        {
            order.Cancel("Payment record creation failed", ChangedByType.System);
            await orderRepository.SaveChangesAsync(cancellationToken);
            return Result<CheckoutResultDto>.Failure(paymentResult.Errors);
        }

        var payment = paymentResult.Value!;
        if (!string.IsNullOrWhiteSpace(session.PaymentIntentId))
            payment.SetStripeReference(session.SessionId, session.PaymentIntentId);

        paymentRepository.Add(payment);
        await paymentRepository.SaveChangesAsync(cancellationToken);

        var domainCart = await cartRepository.GetByCustomerAndStoreAsync(
            customerId, request.StoreId, cancellationToken);
        if (domainCart is not null)
        {
            domainCart.Clear();
            await cartRepository.SaveChangesAsync(cancellationToken);
        }

        return Result<CheckoutResultDto>.Success(
            new CheckoutResultDto(order.Id, session.CheckoutUrl, session.SessionId));
    }

    private static List<Error> ValidateAvailability(
        IReadOnlyList<CheckoutCartItemDto> items)
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

            foreach (var opt in item.Options.Where(o => !o.IsAvailable))
            {
                errors.Add(Error.Validation(
                    "Checkout.OptionUnavailable",
                    $"Option '{opt.NameEn}' on '{item.NameEn}' is no longer available."));
            }
        }

        return errors;
    }
}