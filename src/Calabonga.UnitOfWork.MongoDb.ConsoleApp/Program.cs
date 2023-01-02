using Calabonga.UnitOfWork.MongoDb;
using Calabonga.UnitOfWork.MongoDb.ConsoleApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

var cancellationTokenSource = new CancellationTokenSource();

var logger = container.GetRequiredService<ILogger<Program>>();

var unitOfWork = container.GetService<IUnitOfWork>();
var repository = unitOfWork!.GetRepository<Document>();

logger.LogInformation("{Name}", repository.Collection.CollectionNamespace);

var session = await unitOfWork.GetSessionAsync(cancellationTokenSource.Token);

await DocumentHelper.CreateDocuments(session, repository, logger, cancellationTokenSource.Token);
await DocumentHelper.PrintDocuments(300, false, repository, logger, cancellationTokenSource.Token);
await DocumentHelper.DeleteDocuments(repository, logger, cancellationTokenSource.Token);

logger.LogInformation("Done");

public class CustomNameSelector : ICollectionNameSelector
{
    public string GetMongoCollectionName(string typeName)
    {
        switch (typeName)
        {
            case "Document":
                return "SuperDocument";

            default:
                throw new InvalidOperationException();
        }
    }
}