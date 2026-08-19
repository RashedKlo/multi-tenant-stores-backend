using Domain.Common;
using MediatR;

public record RemoveCartItemCommand(Guid CartItemId, Guid StoreId) : IRequest<Result>;