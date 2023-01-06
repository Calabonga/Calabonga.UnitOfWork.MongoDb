namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Database settings for MongoDb connection
/// </summary>
public class DatabaseSettings : IDatabaseSettings
{
    /// <summary>
    /// Overrides all others settings. Just returns new MongoClient with this connection string
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Your application name
    /// </summary>
    public string ApplicationName { get; set; } = "Untitled";

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = default!;

    /// <summary>
    /// A set of hosts for MongoDb
    /// </summary>
    public string[] Hosts { get; set; } = default!;

    /// <summary>
    /// Replica Set Name
    /// </summary>
    public string? ReplicaSetName { get; set; }

    /// <summary>
    /// MongoDb port. Default is 27017
    /// </summary>
    public int MongoDbPort { get; set; }

    /// <summary>
    /// Enable/Disable verbose logging
    /// </summary>
    public bool VerboseLogging { get; set; }

    /// <summary>
    /// Enable/Disable direct connection
    /// </summary>
    public bool DirectConnection { get; set; }

    /// <summary>
    /// User name and login for connection
    /// </summary>
    public CredentialSettings? Credential { get; set; }

    /// <summary>
    /// Enable/Disable TLS
    /// </summary>
    public bool UseTls { get; set; }
}

/// <summary>
/// Credential data for connection
/// </summary>
public class CredentialSettings
{
    public string? Login { get; set; }

    public string? Password { get; set; }

    public string? Mechanism { get; set; }
}