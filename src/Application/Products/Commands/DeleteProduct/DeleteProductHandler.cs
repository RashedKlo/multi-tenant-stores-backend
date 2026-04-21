using Domain.Interfaces;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Products.Commands.DeleteProduct;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cache;

    public DeleteProductHandler(IProductRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return false;

        _repository.Delete(product);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"products:department:{product.DepartmentId}", cancellationToken);
        await _cache.RemoveAsync("products:all", cancellationToken);

        return true;
    }
}
