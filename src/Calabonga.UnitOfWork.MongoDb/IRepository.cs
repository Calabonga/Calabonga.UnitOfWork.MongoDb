using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Defines the interfaces for generic repository.
/// </summary>
/// <typeparam name="TDocument">The type of the entity.</typeparam>
/// <typeparam name="TType"></typeparam>
public interface IRepository<TDocument, TType> where TDocument : DocumentBase<TType>
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
    /// Returns paged collection of the items using AggregateFacet
    /// </summary>
    /// <param name="pageSize"></param>
    /// <param name="filter"></param>
    /// <param name="sorting"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="pageIndex"></param>
    Task<IPagedList<TDocument>> GetPagedAsync(
        int pageIndex,
        int pageSize,
        FilterDefinition<TDocument> filter,
        SortDefinition<TDocument> sorting,
        CancellationToken cancellationToken);

    /// <summary>
    /// Save data about request to <see cref="ILogger{TCategoryName}"/> as DEBUG <see cref="LogLevel"/> 
    /// </summary>
    /// <param name="requestId"></param>
    void LogRequest(string requestId);
}