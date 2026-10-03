// Domain/interfaces/IUnitOfWork.cs
using Domain.Common;

namespace Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="action"/> inside one DB transaction.
    /// Commits when the returned Result is a success; rolls back on a failed Result or an exception.
    /// </summary>
    Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default);
}