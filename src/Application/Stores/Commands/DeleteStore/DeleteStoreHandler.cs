using Application.Common.Interfaces;
using MediatR;
using Domain.Interfaces;
using Application.Stores.Commands.DeleteStore;

namespace Application.Stores.Commands.DeleteStore;

public class DeleteStoreHandler : IRequestHandler<DeleteStoreCommand, bool>
{
    private readonly IStoreRepository _repository;
    private readonly ICacheService _cache;

    public DeleteStoreHandler(IStoreRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (store is null) return false;

        _repository.Delete(store);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate caches
        await _cache.RemoveAsync("stores:all", cancellationToken);
        await _cache.RemoveAsync($"stores:{request.Id}", cancellationToken);

        return true;
    }
}
