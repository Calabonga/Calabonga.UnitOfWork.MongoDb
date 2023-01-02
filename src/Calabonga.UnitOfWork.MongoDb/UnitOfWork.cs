using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// CALABONGA Warning: do not remove sealed
/// Represents the default implementation of the <see cref="T:IUnitOfWork"/> and <see cref="T:IUnitOfWork{TContext}"/> interface.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDatabaseBuilder _databaseBuilder;
    private bool _disposed;
    private Dictionary<Type, object>? _repositories;

    public UnitOfWork(IDatabaseBuilder databaseBuilder)
    {
        _databaseBuilder = databaseBuilder;
        LastSaveChangesResult = new SaveChangesResult();
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : IDocument
    {
        _repositories ??= new Dictionary<Type, object>();
        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<TEntity>(_databaseBuilder);
        }

        return (IRepository<TEntity>)_repositories[type];
    }

    public SaveChangesResult LastSaveChangesResult { get; }

    public IClientSessionHandle GetSession() => _databaseBuilder.Client.StartSession();
    public Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken) => _databaseBuilder.Client.StartSessionAsync(null, cancellationToken);

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