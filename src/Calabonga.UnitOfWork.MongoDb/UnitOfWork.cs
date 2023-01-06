using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// CALABONGA Warning: do not remove sealed
/// Represents the default implementation of the <see cref="T:IUnitOfWork"/> and <see cref="T:IUnitOfWork{TContext}"/> interface.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ILogger<UnitOfWork> _logger;
    private readonly IDatabaseBuilder _databaseBuilder;
    private bool _disposed;
    private Dictionary<Type, object>? _repositories;

    public UnitOfWork(ILogger<UnitOfWork> logger, IDatabaseBuilder databaseBuilder)
    {
        _logger = logger;
        _databaseBuilder = databaseBuilder;
    }

    public IRepository<TDocument, TType> GetRepository<TDocument, TType>() where TDocument : DocumentBase<TType>
    {
        _repositories ??= new Dictionary<Type, object>();
        var type = typeof(TDocument);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<TDocument, TType>(_databaseBuilder);
        }

        return (IRepository<TDocument, TType>)_repositories[type];
    }

    public void EnsureReplicationSetReady()
    {
        if (_databaseBuilder.Client == null)
        {
            _databaseBuilder.Build();
        }

        _databaseBuilder.Client!.EnsureReplicationSetReady();
    }

    public IClientSessionHandle GetSession()
    {
        if (_databaseBuilder.Client == null)
        {
            _databaseBuilder.Build();
        }
        return _databaseBuilder.Client!.StartSession();
    }

    public Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken)
    {
        if (_databaseBuilder.Client == null)
        {
            _databaseBuilder.Build();
        }
        return _databaseBuilder.Client!.StartSessionAsync(null, cancellationToken);
    }

    /// <summary>
    /// Runs awaitable method in transaction scope. With new instance of repository creation.
    /// </summary>
    /// <typeparam name="TDocument">type of the repository entity</typeparam>
    /// <typeparam name="TType">BsonId type</typeparam>
    /// <param name="taskOperation">operation will run in transaction</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <param name="session">session</param>
    /// <param name="transactionOptions">options</param>
    public async Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, IClientSessionHandle, CancellationToken, Task> taskOperation,
        CancellationToken cancellationToken,
        IClientSessionHandle? session,
        TransactionOptions? transactionOptions = null) where TDocument : DocumentBase<TType>
    {
        using var session1 = session ?? await _databaseBuilder.Build().Client.StartSessionAsync(null, cancellationToken);

        try
        {
            var repository = GetRepository<TDocument, TType>();
            var options = transactionOptions ?? new TransactionOptions(readPreference: ReadPreference.Primary, readConcern: ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority);
            session1.StartTransaction(options);

            await taskOperation(repository, session1, cancellationToken);

            await session1.CommitTransactionAsync(cancellationToken);

        }
        catch (NotSupportedException)
        {
            throw;
        }

        catch (Exception exception)
        {
            await session1.AbortTransactionAsync(cancellationToken);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogError("[TRANSACTION ROLLBACK] {Id} {Message}", session1.ServerSession.Id, exception.Message);
            }

            throw;
        }
    }

    public Task UseTransactionAsync<TDocument, TType>(Func<IRepository<TDocument, TType>, TransactionContext, Task> taskOperation, TransactionContext transactionContext) where TDocument : DocumentBase<TType> => throw new NotImplementedException();

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
    public async Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, IClientSessionHandle, CancellationToken, Task> taskOperation,
        IRepository<TDocument, TType> repository,
        CancellationToken cancellationToken,
        IClientSessionHandle? session,
        TransactionOptions? transactionOptions = null) where TDocument : DocumentBase<TType>
    {
        using var session1 = session ?? await _databaseBuilder.Build().Client.StartSessionAsync(null, cancellationToken);

        try
        {
            var options = transactionOptions ?? new TransactionOptions(readPreference: ReadPreference.Primary, readConcern: ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority);
            session1.StartTransaction(options);

            await taskOperation(repository, session1, cancellationToken);

            await session1.CommitTransactionAsync(cancellationToken);

        }
        catch (NotSupportedException)
        {
            throw;
        }

        catch (Exception exception)
        {
            await session1.AbortTransactionAsync(cancellationToken);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogError("[TRANSACTION ROLLBACK] {Id} {Message}", session1.ServerSession.Id, exception.Message);
            }

            throw;
        }
    }

    /// <summary>
    /// Runs awaitable method in transaction scope. Using instance of the repository already exist.
    /// </summary>
    /// <typeparam name="TDocument">type of the repository entity</typeparam>
    /// <typeparam name="TType">BsonId type</typeparam>
    /// <param name="taskOperation">operation will run in transaction</param>
    /// <param name="repository">instance of the repository</param>
    /// <param name="transactionContext">Transaction context with additional helpful instances for operation</param>
    /// <returns></returns>
    public async Task UseTransactionAsync<TDocument, TType>
    (
        Func<IRepository<TDocument, TType>, TransactionContext, Task> taskOperation,
        IRepository<TDocument, TType> repository,
        TransactionContext transactionContext)
        where TDocument : DocumentBase<TType>
    {
        transactionContext.SetLogger(_logger);

        using var session = transactionContext.Session ?? await _databaseBuilder.Build().Client.StartSessionAsync(null, transactionContext.CancellationToken);

        try
        {
            var options = transactionContext.TransactionOptions ?? new TransactionOptions(readPreference: ReadPreference.Primary, readConcern: ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority);
            session.StartTransaction(options);

            await taskOperation(repository, transactionContext);

            await session.CommitTransactionAsync(transactionContext.CancellationToken);

        }
        catch (NotSupportedException)
        {
            throw;
        }

        catch (Exception exception)
        {
            await session.AbortTransactionAsync(transactionContext.CancellationToken);
            if (transactionContext.Logger.IsEnabled(LogLevel.Information))
            {
                transactionContext.Logger.LogError("[TRANSACTION ROLLBACK] {Id} {Message}", session.ServerSession.Id, exception.Message);
            }

            throw;
        }
    }


    #region Disposable

    public void Dispose()
    {
        Dispose(true);

        // ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">The disposing.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _repositories?.Clear();
            }
        }
        _disposed = true;
    }

    #endregion
}