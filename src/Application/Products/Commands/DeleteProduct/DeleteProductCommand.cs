using MediatR;

namespace Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;
