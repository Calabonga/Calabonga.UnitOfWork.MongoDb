using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// // Calabonga: update summary (2023-01-02 11:54 IMongoDbBuilder)
/// </summary>
public class DatabaseBuilder : IDatabaseBuilder
{
    private readonly ILogger<DatabaseBuilder> _logger;

    public DatabaseBuilder(IMongoDbSettings mongoDbSettings, ICollectionNameSelector collectionNameSelector, ILogger<DatabaseBuilder> logger)
    {
        Settings = mongoDbSettings;
        CollectionNameSelector = collectionNameSelector;
        _logger = logger;
    }

    public ICollectionNameSelector CollectionNameSelector { get; }

    public IMongoDatabase Build()
    {
        Client = BuildMongoClient();
        return Client.GetDatabase(Settings.MongoDbDatabaseName);
    }

    public IMongoClient Client { get; private set; }

    public IMongoDbSettings Settings { get; }

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