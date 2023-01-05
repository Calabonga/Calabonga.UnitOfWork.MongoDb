using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// // Calabonga: update summary (2023-01-04 04:21 IUnitOfWork)
    /// </summary>
    void EnsureReplicationSetReady();

    /// <summary>
    /// Repository for Collection
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <typeparam name="TType"></typeparam>
    /// <returns>MongoDb Collection wrapper as repository</returns>
    IRepository<TDocument, TType> GetRepository<TDocument, TType>() where TDocument : DocumentBase<TType>;

    /// <summary>
    /// // Calabonga: update summary (2023-01-04 04:20 IUnitOfWork)
    /// </summary>
    IClientSessionHandle GetSession();

    /// <summary>
    /// // Calabonga: update summary (2023-01-04 04:22 IUnitOfWork)
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task<IClientSessionHandle>? GetSessionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// // Calabonga: update summary (2023-01-04 04:22 IUnitOfWork)
    /// </summary>
    /// <param name="processOperationWithTransaction"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="session"></param>
    /// <param name="transactionOptions"></param>
    Task UseTransactionAsync(Action<IClientSessionHandle, CancellationToken> processOperationWithTransaction, CancellationToken cancellationToken, IClientSessionHandle? session, TransactionOptions? transactionOptions = null);

    /// <summary>
    /// // Calabonga: update summary (2023-01-04 04:22 IUnitOfWork)
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <typeparam name="TType"></typeparam>
    /// <param name="taskOperation"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="session"></param>
    /// <param name="transactionOptions"></param>
    Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, IClientSessionHandle, CancellationToken, Task> taskOperation,
        CancellationToken cancellationToken,
        IClientSessionHandle? session,
        TransactionOptions? transactionOptions = null)
        where TDocument : DocumentBase<TType>;
}