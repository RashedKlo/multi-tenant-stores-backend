using Application.Features.Coupons.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Coupons.Commands.ApplyCoupon;

public sealed record ApplyCouponCommand(
    Guid StoreId,
    string Code) : IRequest<Result<CouponApplicationDto>>;