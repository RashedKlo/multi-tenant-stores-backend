using Application.Catalog.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetStoreById;

public record GetStoreByIdQuery(Guid StoreId) : IRequest<Result<StoreDetailDto>>;
