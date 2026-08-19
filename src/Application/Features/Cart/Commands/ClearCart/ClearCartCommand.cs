using Domain.Common;
using MediatR;


public record ClearCartCommand(Guid StoreId) : IRequest<Result>;