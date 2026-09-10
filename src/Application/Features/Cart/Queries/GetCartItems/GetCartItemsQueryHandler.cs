using Application.Common.Interfaces;
using Application.Features.Cart.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Features.Cart.Queries.GetCartItems;

public sealed class GetCartItemsQueryHandler
    : IRequestHandler<GetCartItemsQuery, Result<IReadOnlyList<CartItemDto>>>
{
    private readonly ICartQueries _cartQueries;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentLanguageProvider _currentLanguageProvider;

    public GetCartItemsQueryHandler(
        ICartQueries cartQueries,
        ICurrentUserService currentUser,
        ICurrentLanguageProvider currentLanguageProvider)
    {
        _cartQueries = cartQueries;
        _currentUser = currentUser;
        _currentLanguageProvider = currentLanguageProvider;
    }

    public async Task<Result<IReadOnlyList<CartItemDto>>> Handle(
        GetCartItemsQuery request,
        CancellationToken cancellationToken)
    {
        var customerId = _currentUser.IsAuthenticated ? _currentUser.CustomerId : null;
        var guestSessionId = _currentUser.IsAuthenticated ? null : _currentUser.GuestSessionId;

        if (customerId is null && guestSessionId is null)
            return Result<IReadOnlyList<CartItemDto>>.Success(Array.Empty<CartItemDto>());

        var items = await _cartQueries.GetCartItemsAsync(
            customerId,
            guestSessionId,
            _currentLanguageProvider.Language);

        return Result<IReadOnlyList<CartItemDto>>.Success(items);
    }
}