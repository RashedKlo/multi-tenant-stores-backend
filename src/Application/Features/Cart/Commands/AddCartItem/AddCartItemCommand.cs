using Domain.Common;
using MediatR;

public record AddCartItemCommand(
    Guid StoreId,
    Guid ProductId,
    int Quantity,
    string? Notes,
    List<Guid>? OptionIds) : IRequest<Result>;