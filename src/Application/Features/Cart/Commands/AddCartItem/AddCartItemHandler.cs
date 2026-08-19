using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

public class AddCartItemHandler : IRequestHandler<AddCartItemCommand, Result>
{
    private readonly ICartRepository _repo;
    private readonly ICurrentUserService _user;

    public AddCartItemHandler(ICartRepository repo, ICurrentUserService user)
    {
        _repo = repo;
        _user = user;
    }

    public async Task<Result> Handle(AddCartItemCommand request, CancellationToken ct)
    {
        var cartResult = await GetOrCreateCartAsync(request.StoreId, ct);
        if (cartResult.IsFailure)
            return Result.Failure(cartResult.Errors);

        var cart = cartResult.Value!;

        var addResult = cart.AddItem(request.ProductId, request.Quantity, request.Notes, request.OptionIds);
        if (addResult.IsFailure)
            return addResult;

        await _repo.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<Cart>> GetOrCreateCartAsync(Guid storeId, CancellationToken ct)
    {
        if (_user.CustomerId is Guid customerId)
        {
            var existing = await _repo.GetForUpdateByCustomerAndStoreAsync(customerId, storeId, ct);
            if (existing is not null)
                return Result<Cart>.Success(existing);

            var created = Cart.CreateForCustomer(customerId, storeId);
            if (created.IsFailure)
                return created;

            await _repo.AddAsync(created.Value!, ct);
            return created;
        }

        if (_user.GuestSessionId is Guid guestSessionId)
        {
            var existing = await _repo.GetForUpdateByGuestSessionAndStoreAsync(guestSessionId, storeId, ct);
            if (existing is not null)
                return Result<Cart>.Success(existing);

            var created = Cart.CreateForGuest(guestSessionId, storeId);
            if (created.IsFailure)
                return created;

            await _repo.AddAsync(created.Value!, ct);
            return created;
        }

        return Result<Cart>.Failure(
            new Error("Cart.UserNotFound", "No active customer or guest session was found."));
    }
}