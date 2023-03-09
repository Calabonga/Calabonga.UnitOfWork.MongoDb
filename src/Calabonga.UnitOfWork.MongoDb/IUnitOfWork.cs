using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Unit of Work interface for pattern implementation
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Ensures that the MongoDb replica set enabled.
    /// Warning: Do not use this method on the production.
    /// </summary>
    void EnsureReplicationSetReady();

    /// <summary>
    /// Repository for Collection
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    /// <typeparam name="TType"></typeparam>
    /// <returns>MongoDb Collection wrapper as repository</returns>
    IRepository<TDocument, TType> GetRepository<TDocument, TType>(WriteConcern? writeConcern = null,
        ReadConcern? readConcern = null,
        ReadPreference? readPreference = null)
        where TDocument
        : DocumentBase<TType>;

    /// <summary>
    /// Returns session from current client collection <see cref="IMongoClient"/>
    /// </summary>
    IClientSessionHandle GetSession();

    /// <summary>
    /// Returns session from current client collection <see cref="IMongoClient"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Runs awaitable method in transaction scope. With new instance of repository creation.
    /// </summary>
    /// <typeparam name="TDocument">type of the repository entity</typeparam>
    /// <typeparam name="TType">BsonId type</typeparam>
    /// <param name="taskOperation">operation will run in transaction</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <param name="session">session</param>
    /// <param name="transactionOptions">options</param>
    Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, IClientSessionHandle, CancellationToken, Task> taskOperation,
        CancellationToken cancellationToken,
        IClientSessionHandle? session,
        TransactionOptions? transactionOptions = null)
        where TDocument : DocumentBase<TType>;

    /// <summary>
    /// Runs awaitable method in transaction scope. With new instance of repository creation.
    /// </summary>
    /// <typeparam name="TDocument">type of the repository entity</typeparam>
    /// <typeparam name="TType">BsonId type</typeparam>
    /// <param name="taskOperation">operation will run in transaction</param>
    /// <param name="transactionContext">transaction context object</param>
    Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, TransactionContext, Task> taskOperation,
        TransactionContext transactionContext)
        where TDocument : DocumentBase<TType>;

    /// <summary>
    /// Runs awaitable method in transaction scope. Using instance of the repository already exist.
    /// </summary>
    /// <typeparam name="TDocument">type of the repository entity</typeparam>
    /// <typeparam name="TType">BsonId type</typeparam>
    /// <param name="taskOperation">operation will run in transaction</param>
    /// <param name="repository">instance of the repository</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <param name="session">session</param>
    /// <param name="transactionOptions">options</param>
    Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, IClientSessionHandle, CancellationToken, Task> taskOperation,
        IRepository<TDocument, TType> repository,
        CancellationToken cancellationToken,
        IClientSessionHandle? session,
        TransactionOptions? transactionOptions = null) where TDocument : DocumentBase<TType>;

    /// <summary>
    /// Runs awaitable method in transaction scope. Using instance of the repository already exist.
    /// </summary>
    /// <typeparam name="TDocument">type of the repository entity</typeparam>
    /// <typeparam name="TType">BsonId type</typeparam>
    /// <param name="taskOperation">operation will run in transaction</param>
    /// <param name="repository">instance of the repository</param>
    /// <param name="transactionContext">Transaction context with additional helpful instances for operation</param>
    /// <returns></returns>
    Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, TransactionContext, Task> taskOperation,
        IRepository<TDocument, TType> repository,
        TransactionContext transactionContext)
        where TDocument : DocumentBase<TType>;
}

