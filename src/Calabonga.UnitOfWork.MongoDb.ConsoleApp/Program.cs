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
services.Configure<AppSettings>(x => { configuration.GetSection(nameof(AppSettings)).Bind(x); });

#region Configure

//services.AddUnitOfWork(config =>
//{
//    config.MongoDbUserName = "sa";
//    config.MongoDbPassword = "password";
//    config.MongoDbDatabaseName = "MyDatabase";
//    config.MongoDbHosts = new[] { "Localhost" };
//    config.MongoDbPort = 27017;
//    config.MongoDbVerboseLogging = false;
//});

services.AddScoped<ICollectionNameSelector, CustomNameSelector>();

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
    //unitOfWork.EnsureReplicationSetReady();

#endif

    var cancellationTokenSource = new CancellationTokenSource();
    var session = await unitOfWork.GetSessionAsync(cancellationTokenSource.Token);

    await unitOfWork.UseTransactionAsync<OrderBase, int>(ProcessDataInTransaction, cancellationTokenSource.Token, session);

    // await DocumentHelper.CreateDocuments(session, repository, logger, cancellationTokenSource.Token);
    // await DocumentHelper.PrintDocuments(300, true, repository, logger, cancellationTokenSource.Token);
    // await DocumentHelper.DeleteDocuments(repository, logger, cancellationTokenSource.Token);

    logger.LogInformation("Done");

}
catch (Exception exception)
{
    Console.WriteLine(exception.Message);
}

async Task ProcessDataInTransaction(IRepository<OrderBase, int> repositoryInTransaction, IClientSessionHandle session, CancellationToken cancellationToken)
{
    //await repository.Collection.DeleteManyAsync(FilterDefinition<OrderBase>.Empty, cancellationToken);

    var internalOrder1 = DocumentHelper.GetInternal(99);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder1, null, cancellationToken);
    logger.LogInformation("InsertOne: {item1}", internalOrder1);

    var internalOrder2 = DocumentHelper.GetInternal(100);
    await repositoryInTransaction.Collection.InsertOneAsync(session, internalOrder2, null, cancellationToken);
    logger.LogInformation("InsertOne: {item2}", internalOrder2);

    // var filter = Builders<OrderBase>.Filter.Eq(x => x.Id, 99);
    // var updateDefinition = Builders<OrderBase>.Update.Set(x => x.Description, "Updated description");
    // var result = await repositoryInTransaction.Collection.UpdateOneAsync(filter, updateDefinition, new UpdateOptions {IsUpsert = false}, cancellationToken);

    // if (result.IsModifiedCountAvailable)
    // {
    // logger.LogInformation("Update {}", result.ModifiedCount);
    // }
    throw new InvalidOperationException();

}


public class CustomNameSelector : ICollectionNameSelector
{
    public string GetMongoCollectionName(string typeName)
    {
        switch (typeName)
        {
            case nameof(OrderBase):
                return "orders";

            default:
                throw new InvalidOperationException();
        }
    }
}