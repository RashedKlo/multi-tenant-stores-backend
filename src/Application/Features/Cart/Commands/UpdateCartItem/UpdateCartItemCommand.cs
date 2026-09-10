// Application/Features/Cart/Commands/UpdateCartItem/UpdateCartItemCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Cart.Commands.UpdateCartItem;

public sealed record UpdateCartItemCommand(
    Guid CartItemId,
    Guid StoreId,
    int Quantity) : IRequest<Result>;