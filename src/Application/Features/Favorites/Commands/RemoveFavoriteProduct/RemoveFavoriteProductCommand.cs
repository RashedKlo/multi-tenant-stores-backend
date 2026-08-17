using Domain.Common;
using MediatR;

namespace Application.Favorites.Commands.RemoveFavoriteProduct;

public record RemoveFavoriteProductCommand(Guid ProductId) : IRequest<Result>;
