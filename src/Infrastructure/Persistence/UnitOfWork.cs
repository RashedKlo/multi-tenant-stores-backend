// Infrastructure/Persistence/UnitOfWork.cs
using Domain.Common;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Wraps the scoped AppDbContext. All repositories share the same context, so everything
/// done inside <see cref="ExecuteInTransactionAsync"/> (EF changes + ExecuteUpdate/ExecuteSql)
/// belongs to one database transaction.
/// </summary>
public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);

    public Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        // Required if you ever enable EnableRetryOnFailure(); harmless otherwise.
        var strategy = db.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            var result = await action(cancellationToken);

            if (result.IsFailure)
            {
                await tx.RollbackAsync(cancellationToken);
                return result;
            }

            await tx.CommitAsync(cancellationToken);
            return result;
        });
    }
}