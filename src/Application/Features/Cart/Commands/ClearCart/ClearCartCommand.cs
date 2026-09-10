// Application/Features/Cart/Commands/ClearCart/ClearCartCommand.cs
using Domain.Common;
using MediatR;

namespace Application.Features.Cart.Commands.ClearCart;

public sealed record ClearCartCommand(Guid StoreId) : IRequest<Result>;