using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;


public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, Result>
{
    private readonly ICartRepository _repo;
    private readonly ICurrentUserService _user;

    public UpdateCartItemHandler(ICartRepository repo, ICurrentUserService user)
    {
        _repo = repo;
        _user = user;
    }

    public async Task<Result> Handle(UpdateCartItemCommand request, CancellationToken ct)
    {
        var cartResult = await GetCartAsync(request.StoreId, ct);
        if (cartResult.IsFailure)
            return Result.Failure(cartResult.Errors);

        var cart = cartResult.Value!;

        var result = cart.UpdateItemQuantity(request.CartItemId, request.Quantity);
        if (result.IsFailure)
            return result;

        await _repo.SaveChangesAsync(ct);

        return Result.Success();
    }

    private async Task<Result<Cart>> GetCartAsync(Guid storeId, CancellationToken ct)
    {
        if (_user.CustomerId is Guid customerId)
        {
            var existing = await _repo.GetForUpdateByCustomerAndStoreAsync(customerId, storeId, ct);
            if (existing is not null)
                return Result<Cart>.Success(existing);
        }
        else if (_user.GuestSessionId is Guid guestSessionId)
        {
            var existing = await _repo.GetForUpdateByGuestSessionAndStoreAsync(guestSessionId, storeId, ct);
            if (existing is not null)
                return Result<Cart>.Success(existing);
        }

        return Result<Cart>.Failure(
            new Error("Cart.NotFound", "Cart not found."));
    }
}