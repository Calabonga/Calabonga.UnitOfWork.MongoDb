using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb.ConsoleApp;

public static class Helper
{
    private static readonly string[] Centers = { "80", "100" };
    public static OrderInternal GetInternal(int id) => (OrderInternal)GenerateInternalOrders(id, 1).First();

    public static async Task CreateDocuments
    (
        IClientSessionHandle session,
        IRepository<OrderBase, int> repository,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Enable MongoDb profiler
        using var profiler = new MongoDbProfiler(repository, logger);

        const int total = 1_000;
        var internalOrders = GenerateInternalOrders(1, total);
        var externalOrders = GenerateExternalOrders(200_001, total);

        var both = internalOrders.Union(externalOrders).ToList();

        var options1 = new InsertManyOptions { Comment = "07be0e36-f1c3-f6a7-4e52-5333eb32e00e" };
        await repository.Collection.InsertManyAsync(session, both, options1, cancellationToken);

        profiler.LogRequest("07be0e36-f1c3-f6a7-4e52-5333eb32e00e");

        logger.LogInformation("{Created}", both.Count);

        var internalOrders1 = GenerateInternalOrders(400_000, total);
        var externalOrders1 = GenerateExternalOrders(600_001, total);

        var both2 = internalOrders1.Union(externalOrders1).ToList();

        var options2 = new InsertManyOptions { Comment = "7cc66511-f47e-5a89-4a94-cb854b432a0b" };
        await repository.Collection.InsertManyAsync(session, both2, options2, cancellationToken);
        profiler.LogRequest("7cc66511-f47e-5a89-4a94-cb854b432a0b");

        logger.LogInformation("{Created}", both2.Count);
    }

    private static IEnumerable<OrderBase> GenerateInternalOrders(int startFrom = 0, int total = 10)
        => Enumerable.Range(startFrom, total)
            .Select(x => new OrderInternal
            {
                CreatedAt = DateTime.UtcNow,
                Description = $"Description {x}",
                Id = x,
                Numbers = Centers,
                LastNumber = Centers[new Random().Next(0, 2)],
                State = x % 2 == 0 ? DocumentState.Draft : DocumentState.Published,
                Title = $"Title {x}"
            });

    private static IEnumerable<OrderBase> GenerateExternalOrders(int startFrom = 0, int total = 10)
        => Enumerable.Range(startFrom, total)
            .Select(x => new OrderExternal
            {
                CreatedAt = DateTime.UtcNow,
                Description = $"Description {x}",
                Id = x,
                Numbers = Centers,
                LastNumber = Centers[new Random().Next(0, 2)],
                State = x % 2 == 0 ? DocumentState.Draft : DocumentState.Published,
                Title = $"Title {x}"
            });

    public static async Task PrintDocuments
    (
        int pageSize,
        bool showItems,
        IRepository<OrderBase, int> repository,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Enable MongoDb profiler
        using var profiler = new MongoDbProfiler(repository, logger);

        var options1 = new CountOptions { Comment = "07f9c56c-a98a-249c-470c-5e21d807b173" };
        var total = await repository.Collection.CountDocumentsAsync(FilterDefinition<OrderBase>.Empty, options1, cancellationToken: cancellationToken);

        profiler.LogRequest("07f9c56c-a98a-249c-470c-5e21d807b173");
        if (total <= 0)
        {
            logger.LogInformation("No items");
            return;
        }
        for (var i = 1; i <= Math.Ceiling(total / (double)pageSize); i++)
        {
            var paged = await repository.GetPagedAsync(i,
                pageSize,
                Builders<OrderBase>.Filter.Empty,
                Builders<OrderBase>.Sort.Ascending(x => x.CreatedAt),
                cancellationToken);

            logger.LogInformation("Items.Count {Count}", paged.Items.Count);
            if (!showItems)
            {
                continue;
            }

            foreach (var item in paged.Items)
            {
                logger.LogInformation("{Item}", item.ToString());
            }
        }
    }

    public static async Task DeleteDocuments(IRepository<OrderBase, int> repository, ILogger logger,
        CancellationToken cancellationToken)
    {
        // Enable MongoDb profiler
        using var profiler = new MongoDbProfiler(repository, logger);

        var options1 = new CountOptions { Comment = "0266e1ef-59bf-2386-45d8-8d41eb74222b" };
        var total = await repository.Collection.CountDocumentsAsync(FilterDefinition<OrderBase>.Empty, options1, cancellationToken: cancellationToken);
        profiler.LogRequest("0266e1ef-59bf-2386-45d8-8d41eb74222b");
        logger.LogInformation("Deleting all {Total}!", total);

        var options2 = new DeleteOptions { Comment = "b901c15d-782f-0fb6-4d48-de462a6f3381" };
        await repository.Collection.DeleteManyAsync(FilterDefinition<OrderBase>.Empty, options2, cancellationToken);
        profiler.LogRequest("b901c15d-782f-0fb6-4d48-de462a6f3381");

        var options3 = new CountOptions { Comment = "bf3110fc-316d-83af-454c-785eb5bb9a49" };
        var total2 = await repository.Collection.CountDocumentsAsync(FilterDefinition<OrderBase>.Empty, options3, cancellationToken);
        profiler.LogRequest("bf3110fc-316d-83af-454c-785eb5bb9a49");
        logger.LogInformation("After deleting: {Total}", total2);
    }

    public static void PrintDocuments<T>(IEnumerable<T> items, ILogger logger)
    {
        foreach (var item in items)
        {
            logger.LogInformation("{Item}", item?.ToString());
        }
    }

    public static void PrintDocuments<T>(IQueryable<T> items, ILogger logger)
    {
        PrintDocuments(items.AsEnumerable(), logger);
    }

    public static void PrintDocuments<T>(IList<T> items, ILogger logger)
    {
        PrintDocuments(items.AsEnumerable(), logger);
    }
}