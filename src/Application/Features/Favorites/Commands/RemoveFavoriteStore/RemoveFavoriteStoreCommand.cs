using Domain.Common;
using MediatR;

namespace Application.Favorites.Commands.RemoveFavoriteStore;

public record RemoveFavoriteStoreCommand(Guid StoreId) : IRequest<Result>;
