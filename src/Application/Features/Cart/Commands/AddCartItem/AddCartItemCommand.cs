// Application/Features/Cart/Commands/AddCartItem/AddCartItemCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Cart.Commands.AddCartItem;

public sealed record AddCartItemCommand(
    Guid StoreId,
    Guid ProductId,
    int Quantity,
    string? Notes,
    IReadOnlyList<Guid>? OptionIds) : IRequest<Result>;