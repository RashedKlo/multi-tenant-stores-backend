using MediatR;

namespace Application.Stores.Commands.DeleteStore;

public record DeleteStoreCommand(Guid Id) : IRequest<bool>;
