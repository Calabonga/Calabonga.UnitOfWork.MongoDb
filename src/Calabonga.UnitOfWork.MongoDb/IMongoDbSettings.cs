namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// // Calabonga: update summary (2023-01-02 11:53 IMongoDbSettings)
/// </summary>
public interface IMongoDbSettings
{
    string MongoDbDatabaseName { get; }

    string MongoDbUserName { get; }

    string[] MongoDbHosts { get; }

    string MongoDbReplicaSetName { get; }

    string MongoDbPassword { get; }

    int MongoDbPort { get; }

    bool MongoDbVerboseLogging { get; }
}