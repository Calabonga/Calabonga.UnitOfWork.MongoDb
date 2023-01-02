using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb.ConsoleApp;

public static class DocumentHelper
{
    public static async Task CreateDocuments(IClientSessionHandle session, IRepository<Document, int> repository, ILogger logger, CancellationToken cancellationToken)
    {
        var documents = GenerateDocuments(0, 100).ToList();

        foreach (var document in documents)
        {
            await repository.Collection.InsertOneAsync(session, document, null, cancellationToken);
        }

        logger.LogInformation("{Created}", documents.Count);

        var documents2 = GenerateDocuments(1000, 1000).ToList();

        await repository.Collection.InsertManyAsync(session, documents2, null, cancellationToken);

        logger.LogInformation("{Created}", documents2.Count);
    }

    private static IEnumerable<Document> GenerateDocuments(int startFrom = 0, int total = 10)
        => Enumerable.Range(startFrom, total).Select(x => new Document
        {
            CreatedAt = DateTime.UtcNow,
            Description = $"Description {x}",
            Id = x,
            State = x % 2 == 0 ? DocumentState.Draft : DocumentState.Published,
            Title = $"Title {x}"
        });

    public static async Task PrintDocuments(int pageSize, bool showItems, IRepository<Document, int> repository, ILogger logger, CancellationToken cancellationToken)
    {
        var total = await repository.Collection.CountDocumentsAsync(FilterDefinition<Document>.Empty, cancellationToken: cancellationToken);

        for (var i = 0; i < Math.Ceiling(total / (double)pageSize); i++)
        {
            var paged = await repository.GetPagedAsync(i, pageSize, Builders<Document>.Filter.Empty, cancellationToken);
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

    public static async Task DeleteDocuments(IRepository<Document, int> repository, ILogger logger, CancellationToken cancellationToken)
    {
        var total = await repository.Collection.CountDocumentsAsync(FilterDefinition<Document>.Empty, cancellationToken: cancellationToken);
        logger.LogInformation("Deleting all {Total}!", total);

        await repository.Collection.DeleteManyAsync(FilterDefinition<Document>.Empty, cancellationToken);

        logger.LogInformation("After deleting: {Total}", await repository.Collection.CountDocumentsAsync(FilterDefinition<Document>.Empty, null, cancellationToken));
    }
}