using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Reflection;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Database configuration builder
/// </summary>
public sealed class DatabaseBuilder : IDatabaseBuilder
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
    /// <returns>A MongoClient</returns>
    private MongoClient GetMongoClient()
    {
        if (!string.IsNullOrEmpty(Settings.ConnectionString))
        {
            var settings = MongoClientSettings.FromConnectionString(Settings.ConnectionString);
            return new MongoClient(settings);
        }

        var mongoClientSettings = new MongoClientSettings
        {
            Servers = Settings.Hosts.Select(x => new MongoServerAddress(x, Settings.MongoDbPort)).ToArray(),
            ApplicationName = Assembly.GetExecutingAssembly().FullName ?? Settings.ApplicationName
        };


        if (!string.IsNullOrEmpty(Settings.Credential?.Login))
        {
            var internalIdentity = new MongoInternalIdentity("admin", Settings.Credential.Login);
            var passwordEvidence = new PasswordEvidence(Settings.Credential.Password);
            var mongoCredential = new MongoCredential(Settings.Credential.Mechanism, internalIdentity, passwordEvidence);
            mongoClientSettings.Credential = mongoCredential;
        }

        if (!string.IsNullOrWhiteSpace(Settings.ReplicaSetName))
        {
            mongoClientSettings.ReplicaSetName = Settings.ReplicaSetName;
        }

        if (Settings.DirectConnection)
        {
            mongoClientSettings.DirectConnection = true;
        }

        if (Settings.VerboseLogging)
        {
            mongoClientSettings.ClusterConfigurator = clusterBuilder =>
            {
                clusterBuilder.Subscribe<CommandStartedEvent>(e => { _logger.LogDebug($"{e.CommandName} - {e.Command.ToJson()}"); });
            };
        }

        if (Settings.UseTls)
        {
            mongoClientSettings.UseTls = true;
        }

        return new MongoClient(mongoClientSettings);
    }
}