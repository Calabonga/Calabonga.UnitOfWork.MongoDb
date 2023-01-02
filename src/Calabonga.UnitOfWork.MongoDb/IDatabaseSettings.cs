namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// MongoDb settings for connection
/// </summary>
public interface IDatabaseSettings
{
    string DatabaseName { get; }

    string MongoDbUserName { get; }

    string[] MongoDbHosts { get; }

    string MongoDbReplicaSetName { get; }

    string MongoDbPassword { get; }

    int MongoDbPort { get; }

    bool MongoDbVerboseLogging { get; }
}