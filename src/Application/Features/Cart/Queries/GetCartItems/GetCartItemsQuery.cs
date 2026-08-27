using Application.Common.Models;
using MediatR;

namespace Application.Carts.Queries.GetCartItems;


public sealed record GetCartItemsQuery : IRequest<IReadOnlyList<CartItemDto>>;