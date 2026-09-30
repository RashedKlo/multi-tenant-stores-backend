using Application.Catalog.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreCoupons;

public sealed record GetStoreCouponsQuery(Guid StoreId) : IRequest<Result<IReadOnlyList<StoreCouponDto>>>;