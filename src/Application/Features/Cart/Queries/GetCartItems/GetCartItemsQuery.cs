using Application.Features.Cart.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Cart.Queries.GetCartItems;

public sealed record GetCartItemsQuery : IRequest<Result<IReadOnlyList<CartItemDto>>>;