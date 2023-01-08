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

    /// <summary>
    /// Database name
    /// </summary>
    string DatabaseName { get; }

    /// <summary>
    /// A set of hosts for MongoDb
    /// </summary>
    string[] Hosts { get; }

    /// <summary>
    /// Replica Set Name
    /// </summary>
    string ReplicaSetName { get; }

    /// <summary>
    /// MongoDb port. Default is 27017
    /// </summary>
    int MongoDbPort { get; }

    /// <summary>
    /// Enable/Disable verbose logging
    /// </summary>
    bool VerboseLogging { get; }

    /// <summary>
    /// Your application name
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// Enable/Disable direct connection
    /// </summary>
    bool DirectConnection { get; }

    /// <summary>
    /// User name and login for connection
    /// </summary>
    CredentialSettings? Credential { get; }

    /// <summary>
    /// Enable/Disable TLS
    /// </summary>
    bool UseTls { get; }
}