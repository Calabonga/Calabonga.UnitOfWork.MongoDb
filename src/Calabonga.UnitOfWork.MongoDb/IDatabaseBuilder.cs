using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Database builder from database settings
/// </summary>
public interface IDatabaseBuilder
{
    ICollectionNameSelector CollectionNameSelector { get; }

    IMongoDatabase Build();

    IMongoClient? Client { get; }

    IDatabaseSettings Settings { get; }
}