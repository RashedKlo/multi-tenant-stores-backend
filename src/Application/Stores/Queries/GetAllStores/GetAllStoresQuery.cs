using Application.Stores.DTOs;
using MediatR;

namespace Application.Stores.Queries.GetAllStores;

public record GetAllStoresQuery() : IRequest<List<StoreDto>>;
