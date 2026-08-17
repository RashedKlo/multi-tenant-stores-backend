using Domain.Common;
using MediatR;

namespace Application.Favorites.Commands.AddFavoriteStore;

public record AddFavoriteStoreCommand(Guid StoreId) : IRequest<Result>;
