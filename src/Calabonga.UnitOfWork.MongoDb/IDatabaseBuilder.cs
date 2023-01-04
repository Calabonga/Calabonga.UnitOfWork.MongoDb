using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// // Calabonga: update summary (2023-01-02 11:54 IMongoDbBuilder)
/// </summary>
public interface IDatabaseBuilder
{
    ICollectionNameSelector CollectionNameSelector { get; }

    IMongoDatabase Build();

    IMongoClient? Client { get; }

    IDatabaseSettings Settings { get; }
}