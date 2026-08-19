using Domain.Common;
using MediatR;

public record UpdateCartItemCommand(Guid CartItemId, Guid StoreId, int Quantity) : IRequest<Result>;
