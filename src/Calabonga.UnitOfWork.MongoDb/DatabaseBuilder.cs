using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Reflection;

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
        Client = GetMongoClient();
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
    private MongoClient GetMongoClient()
    {
        if (!string.IsNullOrEmpty(Settings.ConnectionString))
        {
            return new MongoClient(Settings.ConnectionString);
        }

        var mongoClientSettings = new MongoClientSettings
        {
            Servers = Settings.MongoDbHosts.Select(x => new MongoServerAddress(x, Settings.MongoDbPort)).ToArray(),
            ApplicationName = Assembly.GetExecutingAssembly().FullName ?? Settings.ApplicationName
        };


        if (!string.IsNullOrEmpty(Settings.Credential?.MongoDbUserName))
        {
            mongoClientSettings.Credential = MongoCredential.CreateCredential(Settings.DatabaseName, Settings.Credential.MongoDbUserName, Settings.Credential.MongoDbPassword);
        }

        if (!string.IsNullOrWhiteSpace(Settings.MongoDbReplicaSetName))
        {
            mongoClientSettings.ReplicaSetName = Settings.MongoDbReplicaSetName;
        }

        if (Settings.DirectConnection)
        {
            mongoClientSettings.DirectConnection = true;
        }

        if (Settings.MongoDbVerboseLogging)
        {
            mongoClientSettings.ClusterConfigurator = clusterBuilder =>
            {
                clusterBuilder.Subscribe<CommandStartedEvent>(e => { _logger.LogDebug($"{e.CommandName} - {e.Command.ToJson()}"); });
            };
        }

        mongoClientSettings.UseTls = false;

        // return new MongoClient("mongodb://localhost:27017/?readPreference=primary&ssl=false&directConnection=true");
        return new MongoClient(mongoClientSettings);
    }
}