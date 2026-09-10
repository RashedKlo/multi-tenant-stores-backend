// Application/Features/Cart/Commands/RemoveCartItem/RemoveCartItemHandler.cs
using Application.Common.Extensions;
using Application.Common.Interfaces;
using Domain.Aggregates.Cart;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Cart.Commands.RemoveCartItem;

public sealed class RemoveCartItemHandler : IRequestHandler<RemoveCartItemCommand, Result>
{
    private readonly ICartRepository _carts;
    private readonly ICurrentUserService _user;

    public RemoveCartItemHandler(ICartRepository carts, ICurrentUserService user)
    {
        _carts = carts;
        _user = user;
    }

    public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken ct)
    {
        var cart=_user.CustomerId is not null?
                      await _carts.GetByCustomerAndStoreAsync(_user.CustomerId.Value, request.StoreId, ct)
                :_user.GuestSessionId is not null?
                      await _carts.GetByGuestAndStoreAsync(_user.GuestSessionId.Value, request.StoreId, ct)
                :null;
                

        if (cart is null)
            return Result.Failure(Error.NotFound("Cart.NotFound", "Cart not found."));

        var result = cart.RemoveItem(request.CartItemId);
        if (result.IsFailure)
            return result;

        await _carts.SaveChangesAsync(ct);
        return Result.Success();
    }

}