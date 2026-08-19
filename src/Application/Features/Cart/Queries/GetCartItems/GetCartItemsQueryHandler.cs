// Application/Carts/Queries/GetCartItems/GetCartItemsQueryHandler.cs
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;

namespace Application.Carts.Queries.GetCartItems;

public sealed class GetCartItemsQueryHandler 
    : IRequestHandler<GetCartItemsQuery, IReadOnlyList<CartItemDto>>
{
    private readonly ICartQueries _cartQueries;

    public GetCartItemsQueryHandler(ICartQueries cartQueries)
        => _cartQueries = cartQueries;

    public async Task<IReadOnlyList<CartItemDto>> Handle(
        GetCartItemsQuery request,
        CancellationToken cancellationToken)
        =>  await _cartQueries.GetCartItemsAsync(request.CartId);
}