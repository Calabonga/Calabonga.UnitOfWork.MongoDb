namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// MongoDb settings for connection
/// </summary>
public interface IDatabaseSettings
{
    /// <summary>
    /// Overrides all others settings. Just returns new MongoClient with this connection string
    /// </summary>
    string? ConnectionString { get; set; }

    string DatabaseName { get; }

    string[] MongoDbHosts { get; }

    string MongoDbReplicaSetName { get; }

    int MongoDbPort { get; }

    bool MongoDbVerboseLogging { get; }

    string ApplicationName { get; }

    bool DirectConnection { get; }

    CredentialSettings? Credential { get; }
}