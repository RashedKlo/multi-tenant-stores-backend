// Application/Features/Cart/Commands/RemoveCartItem/RemoveCartItemCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Cart.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(
    Guid CartItemId,
    Guid StoreId) : IRequest<Result>;