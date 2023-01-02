using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

public interface IUnitOfWork : IDisposable
{
    IRepository<TDocument> GetRepository<TDocument>() where TDocument : IDocument;

    /// <summary>
    /// Last error after SaveChanges operation executed
    /// </summary>
    SaveChangesResult LastSaveChangesResult { get; }

    IClientSessionHandle GetSession();

    Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken);
}