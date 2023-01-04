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

    public void EnsureReplicationSetReady() => _databaseBuilder.Client.EnsureReplicationSetReady();

    public IClientSessionHandle GetSession() => _databaseBuilder.Client.StartSession();
    public Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken) => _databaseBuilder.Client.StartSessionAsync(null, cancellationToken);

    public async Task UseTransactionAsync(Action<IClientSessionHandle, CancellationToken> processOperationWithTransaction, CancellationToken cancellationToken, IClientSessionHandle? session, TransactionOptions? transactionOptions = null)
    {
        using var session1 = session ?? await _databaseBuilder.Build().Client.StartSessionAsync(null, cancellationToken);

        try
        {
            var options = transactionOptions ?? new TransactionOptions(readConcern: ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority);
            session1.StartTransaction(options);
            processOperationWithTransaction(session1, cancellationToken);
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
            var collection = GetRepository<TDocument, TType>();
            var options = transactionOptions ?? new TransactionOptions(readPreference: ReadPreference.Primary, readConcern: ReadConcern.Snapshot, writeConcern: WriteConcern.WMajority);
            session1.StartTransaction(options);

            await taskOperation(collection, session1, cancellationToken);

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

}