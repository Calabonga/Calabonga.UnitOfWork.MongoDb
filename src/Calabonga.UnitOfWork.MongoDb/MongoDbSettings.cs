namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// // Calabonga: update summary (2023-01-02 11:53 IMongoDbSettings)
/// </summary>
public class MongoDbSettings : IMongoDbSettings
{
    public string MongoDbDatabaseName { get; set; }

    public string? MongoDbUserName { get; set; }

    public string[] MongoDbHosts { get; set; }

    public string? MongoDbReplicaSetName { get; set; }

    public string? MongoDbPassword { get; set; }

    public int MongoDbPort { get; set; }

    public bool MongoDbVerboseLogging { get; set; }
}