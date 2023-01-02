using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Defines the interfaces for generic repository.
/// </summary>
/// <typeparam name="TDocument">The type of the entity.</typeparam>
public interface IRepository<TDocument> where TDocument : IDocument
{
    #region MongoDb base

    /// <summary>Gets the namespace of the collection.</summary>
    CollectionNamespace CollectionNamespace { get; }
    /// <summary>Gets the database.</summary>
    IMongoDatabase Database { get; }
    /// <summary>Gets the document serializer.</summary>
    IBsonSerializer<TDocument> DocumentSerializer { get; }
    /// <summary>Gets the index manager.</summary>
    IMongoIndexManager<TDocument> Indexes { get; }
    /// <summary>Gets the settings.</summary>
    MongoCollectionSettings Settings { get; }

    IMongoCollection<TDocument> Collection { get; }

    #endregion

    /// <summary>
    /// // Calabonga: update summary (2023-01-02 03:24 IRepository)
    /// </summary>
    /// <param name="pageSize"></param>
    /// <param name="filter"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="options"></param>
    /// <param name="pageIndex"></param>
    /// <returns></returns>
    Task<IPagedList<TDocument>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        FilterDefinition<TDocument> filter,
        CancellationToken cancellationToken,
        FindOptions<TDocument>? options = null);
}