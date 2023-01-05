using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb.ConsoleApp;

public static class DocumentHelper
{
    public static OrderInternal GetInternal(int id) => (OrderInternal)GenerateInternalOrders(id, 1).First();

    public static async Task CreateDocuments(IClientSessionHandle session, IRepository<OrderBase, int> repository, ILogger logger, CancellationToken cancellationToken)
    {
        var internalOrders = GenerateInternalOrders(1);
        var externalOrders = GenerateExternalOrders(20);

        var both = internalOrders.Union(externalOrders).ToList();

        foreach (var document in both)
        {
            await repository.Collection.InsertOneAsync(session, document, null, cancellationToken);
        }

        logger.LogInformation("{Created}", both.Count);

        var internalOrders1 = GenerateInternalOrders(1000, 1000);
        var externalOrders1 = GenerateExternalOrders(2000, 1000);

        var both2 = internalOrders1.Union(externalOrders1).ToList();

        await repository.Collection.InsertManyAsync(session, both2, null, cancellationToken);

        logger.LogInformation("{Created}", both2.Count);
    }

    private static IEnumerable<OrderBase> GenerateInternalOrders(int startFrom = 0, int total = 10)
        => Enumerable.Range(startFrom, total)
            .Select(x => new OrderInternal
            {
                CreatedAt = DateTime.UtcNow,
                Description = $"Description {x}",
                Id = x,
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
                State = x % 2 == 0 ? DocumentState.Draft : DocumentState.Published,
                Title = $"Title {x}"
            });

    public static async Task PrintDocuments(int pageSize, bool showItems, IRepository<OrderBase, int> repository, ILogger logger, CancellationToken cancellationToken)
    {
        var total = await repository.Collection.CountDocumentsAsync(FilterDefinition<OrderBase>.Empty, cancellationToken: cancellationToken);

        for (var i = 0; i < Math.Ceiling(total / (double)pageSize); i++)
        {
            var paged = await repository.GetPagedAsync(i, pageSize, Builders<OrderBase>.Filter.Empty, cancellationToken);
            logger.LogInformation("Items.Count {Count}", paged.Items.Count);
            if (!showItems)
            {
                continue;
            }

            foreach (var item in paged.Items)
            {
                logger.LogInformation(item.ToString());
            }
        }

    }

    public static async Task DeleteDocuments(IRepository<OrderBase, int> repository, ILogger logger, CancellationToken cancellationToken)
    {
        var total = await repository.Collection.CountDocumentsAsync(FilterDefinition<OrderBase>.Empty, cancellationToken: cancellationToken);
        logger.LogInformation("Deleting all {Total}!", total);

        await repository.Collection.DeleteManyAsync(FilterDefinition<OrderBase>.Empty, cancellationToken);

        logger.LogInformation("After deleting: {Total}", await repository.Collection.CountDocumentsAsync(FilterDefinition<OrderBase>.Empty, null, cancellationToken));
    }
}