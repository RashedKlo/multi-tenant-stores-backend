using Application.Stores.DTOs;
using MediatR;

namespace Application.Stores.Queries.GetStoreById;

public record GetStoreByIdQuery(Guid Id) : IRequest<StoreDto?>;
