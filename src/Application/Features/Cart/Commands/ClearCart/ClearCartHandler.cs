// Application/Features/Cart/Commands/ClearCart/ClearCartHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Cart.Commands.ClearCart;

public sealed class ClearCartHandler : IRequestHandler<ClearCartCommand, Result>
{
    private readonly ICartRepository _carts;
    private readonly ICurrentUserService _user;

    public ClearCartHandler(ICartRepository carts, ICurrentUserService user)
    {
        _carts = carts;
        _user = user;
    }

    public async Task<Result> Handle(ClearCartCommand request, CancellationToken ct)
    {
        var cart=_user.CustomerId is not null?
                      await _carts.GetByCustomerAndStoreAsync(_user.CustomerId.Value, request.StoreId, ct)
                :_user.GuestSessionId is not null?
                      await _carts.GetByGuestAndStoreAsync(_user.GuestSessionId.Value, request.StoreId, ct)
                :null;
                

        if (cart is null)
            return Result.Failure(Error.NotFound("Cart.NotFound", "Cart not found."));


        var result = cart.Clear();
        if (result.IsFailure)
            return result;

        await _carts.SaveChangesAsync(ct);
        return Result.Success();
    }
 
}