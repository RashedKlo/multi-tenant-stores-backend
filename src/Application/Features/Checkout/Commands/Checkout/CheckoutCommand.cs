
using Application.Checkout.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Checkout.Commands.Checkout;

public sealed record CheckoutCommand(
    Guid StoreId,
    Guid AddressId,
    string? DeliveryPhone = null,
    string? CouponCode = null) : IRequest<Result<CheckoutResultDto>>;