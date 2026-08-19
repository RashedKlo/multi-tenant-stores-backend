// Application/Carts/Queries/GetCartItems/GetCartItemsQuery.cs
using Application.Common.Models;
using MediatR;

namespace Application.Carts.Queries.GetCartItems;

public sealed record GetCartItemsQuery(Guid CartId) : IRequest<IReadOnlyList<CartItemDto>>;