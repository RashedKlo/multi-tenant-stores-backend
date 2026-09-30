using Application.Common.Interfaces;
using Application.Catalog.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreCoupons;

public sealed class GetStoreCouponsHandler(ICatalogQueries catalogQueries)
    : IRequestHandler<GetStoreCouponsQuery, Result<IReadOnlyList<StoreCouponDto>>>
{
    public async Task<Result<IReadOnlyList<StoreCouponDto>>> Handle(
        GetStoreCouponsQuery request,
        CancellationToken cancellationToken)
    {
        var coupons = await catalogQueries.GetStoreCouponsAsync(request.StoreId, cancellationToken);
        return Result<IReadOnlyList<StoreCouponDto>>.Success(coupons);
    }
}