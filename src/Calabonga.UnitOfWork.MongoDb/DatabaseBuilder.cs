using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Database configuration builder
/// </summary>
public class DatabaseBuilder : IDatabaseBuilder
{
    private readonly ILogger<DatabaseBuilder> _logger;

    public DatabaseBuilder(IDatabaseSettings databaseSettings, ICollectionNameSelector collectionNameSelector, ILogger<DatabaseBuilder> logger)
    {
        Settings = databaseSettings;
        CollectionNameSelector = collectionNameSelector;
        _logger = logger;
    }

    public ICollectionNameSelector CollectionNameSelector { get; }

    public IMongoDatabase Build()
    {
        Client = BuildMongoClient();
        return Client.GetDatabase(Settings.DatabaseName);
    }

    /// <summary>
    /// MongoClient
    /// </summary>
    public IMongoClient Client { get; private set; }

    /// <summary>
    /// MongoDb database settings
    /// </summary>
    public IDatabaseSettings Settings { get; }

    /// <summary>
    /// Build MongoDb client base on <see cref="IDatabaseSettings"/>
    /// </summary>
    /// <returns></returns>
    private MongoClient BuildMongoClient()
    {
        var mongoClientSettings = new MongoClientSettings
        {
            Servers = Settings.MongoDbHosts.Select(x => new MongoServerAddress(x, Settings.MongoDbPort)).ToArray(),
        };

        if (!string.IsNullOrEmpty(Settings.MongoDbUserName))
        {
            mongoClientSettings.Credential = MongoCredential.CreateCredential(null, Settings.MongoDbUserName, Settings.MongoDbPassword);
        }

        if (!string.IsNullOrWhiteSpace(Settings.MongoDbReplicaSetName))
        {
            mongoClientSettings.ReplicaSetName = Settings.MongoDbReplicaSetName;
        }

        if (Settings.MongoDbVerboseLogging)
        {
            mongoClientSettings.ClusterConfigurator = clusterBuilder =>
            {
                clusterBuilder.Subscribe<CommandStartedEvent>(e => { _logger.LogDebug($"{e.CommandName} - {e.Command.ToJson()}"); });
            };
        }

        return new MongoClient(mongoClientSettings);
    }
}