// Application/Features/Cart/Commands/AddCartItem/AddCartItemHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Cart.Commands.AddCartItem;

public sealed class AddCartItemHandler : IRequestHandler<AddCartItemCommand, Result>
{
    private readonly ICartRepository _carts;
    private readonly ICurrentUserService _user;

    public AddCartItemHandler(ICartRepository carts, ICurrentUserService user)
    {
        _carts = carts;
        _user = user;
    }

    public async Task<Result> Handle(AddCartItemCommand request, CancellationToken ct)
    {
        var cartResult = await GetOrCreateAsync(request.StoreId, ct);
        if (cartResult.IsFailure)
            return Result.Failure(cartResult.Errors);

        var addResult = cartResult.Value!.AddItem(
            request.ProductId,
            request.Quantity,
            request.Notes,
            request.OptionIds);

        if (addResult.IsFailure)
            return addResult;

        await _carts.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<Domain.Aggregates.Cart.Cart>> GetOrCreateAsync(Guid StoreId, CancellationToken ct)
    {
       var cart=_user.CustomerId is not null?
                      await _carts.GetByCustomerAndStoreAsync(_user.CustomerId.Value,StoreId, ct)
                :_user.GuestSessionId is not null?
                      await _carts.GetByGuestAndStoreAsync(_user.GuestSessionId.Value,StoreId, ct)
                :null;
        
        if (cart is not null)
            return Result<Domain.Aggregates.Cart.Cart>.Success(cart);
        var created = Domain.Aggregates.Cart.Cart.Create(StoreId, _user.CustomerId, _user.GuestSessionId);
       
        if (created.IsFailure)
            return created;

        await _carts.AddAsync(created.Value!, ct);
        return created;
    }
}