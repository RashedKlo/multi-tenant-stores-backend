using Domain.Common;
using MediatR;

namespace Application.Addresses.Commands.DeleteAddress;

public record DeleteAddressCommand(Guid Id) : IRequest<Result<bool>>;
