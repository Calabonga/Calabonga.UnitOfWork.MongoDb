using Calabonga.UnitOfWork.MongoDb;
using Calabonga.UnitOfWork.MongoDb.ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Serilog;

#region configuration

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder().AddJsonFile("appSettings.json", optional: false, reloadOnChange: false).Build();
var loggerConfiguration = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();
services.AddLogging(x => x.AddSerilog(loggerConfiguration));

#region Configure

//services.AddUnitOfWork(config =>
//{
//    //config.Credential = new CredentialSettings { Login = "mongo", Password = "mongo", Mechanism = "SCRAM-SHA-1" };
//    config.DatabaseName = "MyDatabase";
//    config.Hosts = new[] { "localhost" };
//    config.MongoDbPort = 27017;
//    config.DirectConnection = true;
//    config.ReplicaSetName = "rs0";
//    config.ApplicationName = "Demo";
//    config.VerboseLogging = false;
//    config.UseTls = false;
//});

services.AddScoped<ICollectionNameSelector, MyCollectionNameSelector>();

services.AddUnitOfWork(configuration.GetSection(nameof(DatabaseSettings)));

#endregion

var container = services.BuildServiceProvider();

#endregion

var logger = container.GetRequiredService<ILogger<Program>>();
var unitOfWork = container.GetService<IUnitOfWork>();
var repository = unitOfWork!.GetRepository<OrderBase, int>();

logger.LogInformation("Application started with Namespace: {Name}", repository.Collection.CollectionNamespace);

try
{

#if DEBUG

    // Ensure Replication Set enabled
    // unitOfWork.EnsureReplicationSetReady();

#endif

    var cancellationTokenSource = new CancellationTokenSource();
    var session = await unitOfWork.GetSessionAsync(cancellationTokenSource.Token);

    await unitOfWork.UseTransactionAsync<OrderBase, int>(ProcessDataInTransactionAsync, cancellationTokenSource.Token, session);
    await unitOfWork.UseTransactionAsync<OrderBase, int>(ProcessDataInTransactionAsync4, new TransactionContext(new TransactionOptions(), session, cancellationTokenSource.Token));
    await unitOfWork.UseTransactionAsync(ProcessDataInTransactionAsync2, repository, cancellationTokenSource.Token, session);
    await unitOfWork.UseTransactionAsync(ProcessDataInTransactionAsync3, repository, new TransactionContext(new TransactionOptions(), session, cancellationTokenSource.Token));

    logger.LogInformation("Done");

}
catch (Exception exception)
{
    logger.LogError(exception.Message);
}


#region Transaction
async Task ProcessDataInTransactionAsync4(IRepository<OrderBase, int> repositoryInTransaction, TransactionContext transactionContext)
{
    await repositoryInTransaction.Collection.DeleteManyAsync(FilterDefinition<OrderBase>.Empty, transactionContext.CancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder1, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder2, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var result = await repositoryInTransaction.Collection.UpdateOneAsync(transactionContext.Session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, transactionContext.CancellationToken);

    if (result.IsModifiedCountAvailable)
    {
        transactionContext.Logger.LogInformation("Update {}", result.ModifiedCount);
    }

    throw new InvalidOperationException();
}

async Task ProcessDataInTransactionAsync3(IRepository<OrderBase, int> repositoryInTransaction, TransactionContext transactionContext)
{
    await repositoryInTransaction.Collection.DeleteManyAsync(FilterDefinition<OrderBase>.Empty, transactionContext.CancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder1, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(transactionContext.Session, internalOrder2, null, transactionContext.CancellationToken);
    transactionContext.Logger.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var result = await repositoryInTransaction.Collection.UpdateOneAsync(transactionContext.Session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, transactionContext.CancellationToken);

    if (result.IsModifiedCountAvailable)
    {
        transactionContext.Logger.LogInformation("Update {}", result.ModifiedCount);
    }

    throw new InvalidOperationException();
}

async Task ProcessDataInTransactionAsync2(IRepository<OrderBase, int> repositoryInTransaction, IClientSessionHandle session, CancellationToken cancellationToken)
{
    await repository.Collection.DeleteManyAsync(FilterDefinition<OrderBase>.Empty, cancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder1, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder2, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var result = await repositoryInTransaction.Collection.UpdateOneAsync(session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, cancellationToken);

    if (result.IsModifiedCountAvailable)
    {
        logger!.LogInformation("Update {}", result.ModifiedCount);
    }

    throw new InvalidOperationException();
}

async Task ProcessDataInTransactionAsync(IRepository<OrderBase, int> repositoryInTransaction, IClientSessionHandle session, CancellationToken cancellationToken)
{
    await repository.Collection.DeleteManyAsync(FilterDefinition<OrderBase>.Empty, cancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder1, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder2, null, cancellationToken);
    logger!.LogInformation("InsertOne: {item2}", internalOrder2);

    var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    var result = await repositoryInTransaction.Collection
    .UpdateOneAsync(session, filter, updateDefinition, new UpdateOptions { IsUpsert = false }, cancellationToken);

    if (result.IsModifiedCountAvailable)
    {
        logger!.LogInformation("Update {}", result.ModifiedCount);
    }

    throw new InvalidOperationException();
}

#endregion


public class MyCollectionNameSelector : ICollectionNameSelector
{
    public string GetMongoCollectionName(string typeName)
    {
        switch (typeName)
        {
            case "OrderBase":
                return "orders";
        }

        return typeName;
    }
}