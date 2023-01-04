namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// // Calabonga: update summary (2023-01-02 11:53 IMongoDbSettings)
/// </summary>
public class DatabaseSettings : IDatabaseSettings
{
    public string? ConnectionString { get; set; }

    public string ApplicationName { get; set; } = "Untitled";

    public string DatabaseName { get; set; } = default!;

    public string[] MongoDbHosts { get; set; } = default!;

    public string? MongoDbReplicaSetName { get; set; }

    public int MongoDbPort { get; set; }

    public bool MongoDbVerboseLogging { get; set; }

    public bool DirectConnection { get; set; }

    public CredentialSettings? Credential { get; set; }
}

/// <summary>
/// // Calabonga: update summary (2023-01-04 03:55 DatabaseSettings)
/// </summary>
public class CredentialSettings
{
    public string? MongoDbUserName { get; set; }

    public string? MongoDbPassword { get; set; }
}