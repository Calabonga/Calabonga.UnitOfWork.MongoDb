using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb
{
    /// <summary>
    /// Extensions for <see cref="IMongoClient"/>
    /// </summary>
    public static class MongoClientExtension
    {
        /// <summary>
        /// Tests that a transaction available in MongoDb replica set
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="cancellation"></param>
        public static async Task EnsureReplicationSetReadyAsync(
            this IMongoClient mongoClient,
            CancellationToken cancellation)
        {

            var database = mongoClient.GetDatabase("__empty-db");
            try
            {
                while (true)
                {
                    _ = database.GetCollection<Empty>("__empty");
                    await database.DropCollectionAsync("__empty", cancellation);

                    var session = await mongoClient.StartSessionAsync(cancellationToken: cancellation);

                    try
                    {
                        session.StartTransaction();
                        await session.AbortTransactionAsync(cancellationToken: cancellation);
                    }
                    finally
                    {
                        session.Dispose();
                    }

                    break;
                }
            }
            finally
            {
                await mongoClient
                    .DropDatabaseAsync("__empty-db", cancellationToken: default)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Tests that a transaction available in MongoDb replica set
        /// </summary>
        /// <param name="mongoClient"></param>
        public static void EnsureReplicationSetReady(this IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("__empty-db");
            try
            {
                while (true)
                {

                    _ = database.GetCollection<Empty>("__empty");
                    database.DropCollection("__empty");

                    var session = mongoClient.StartSession();

                    try
                    {
                        session.StartTransaction();
                        session.AbortTransaction();
                    }
                    finally
                    {
                        session.Dispose();
                    }

                    break;
                }
            }
            finally
            {
                mongoClient.DropDatabase("__empty-db");
            }
        }

        private sealed class Empty
        {
            public int Id { get; set; }
        }
    }
}
