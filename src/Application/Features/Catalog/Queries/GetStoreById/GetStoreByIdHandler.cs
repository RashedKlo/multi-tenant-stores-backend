using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreById;

public sealed class GetStoreByIdHandler(
    ICatalogQueries catalogQueries,
    ICurrentUserService currentUser,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetStoreByIdQuery, Result<StoreDetailDto>>
{
    public async Task<Result<StoreDetailDto>> Handle(
        GetStoreByIdQuery request,
        CancellationToken cancellationToken)
    {
        var store = await catalogQueries.GetStoreByIdAsync(
            request.StoreId,
            currentUser.CustomerId,
            currentLanguageProvider.Language,
            cancellationToken);

        return store is null
            ? Result<StoreDetailDto>.Failure(Error.NotFound("Store.NotFound", "Store not found."))
            : Result<StoreDetailDto>.Success(store);
    }
}
