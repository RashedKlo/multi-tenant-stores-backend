using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Carts.Queries.GetCartItems;

public sealed class GetCartItemsQueryHandler
    : IRequestHandler<GetCartItemsQuery, IReadOnlyList<CartItemDto>>
{
    private readonly ICartQueries _cartQueries;
    private readonly ICurrentUserService _currentUser;

    public GetCartItemsQueryHandler(ICartQueries cartQueries, ICurrentUserService currentUser)
    {
        _cartQueries = cartQueries;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CartItemDto>> Handle(
        GetCartItemsQuery request,
        CancellationToken cancellationToken)
    {
        // Authenticated wins outright; only fall back to guest session
        // when there's no authenticated customer.
        var customerId = _currentUser.IsAuthenticated ? _currentUser.CustomerId : null;
        var guestSessionId = _currentUser.IsAuthenticated ? null : _currentUser.GuestSessionId;

        // No identity at all (e.g. anonymous request with no guest
        // session cookie yet) — nothing to look up.
        if (customerId is null && guestSessionId is null)
            return [];

        return await _cartQueries.GetCartItemsAsync(customerId, guestSessionId);
    }
}