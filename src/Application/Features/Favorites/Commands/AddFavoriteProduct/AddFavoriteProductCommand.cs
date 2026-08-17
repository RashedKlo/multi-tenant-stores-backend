using Domain.Common;
using MediatR;

namespace Application.Favorites.Commands.AddFavoriteProduct;

public record AddFavoriteProductCommand(Guid ProductId) : IRequest<Result>;
