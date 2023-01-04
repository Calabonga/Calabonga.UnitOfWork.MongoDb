using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb
{
    /// <summary>
    /// // Calabonga: update summary (2023-01-04 04:42 MongoClientExtension)
    /// </summary>
    public static class MongoClientExtension
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(5000);

        public static async Task EnsureReplicationSetReadyAsync(
            this IMongoClient mongoClient,
            CancellationToken cancellation)
        {
            var delay = InitialDelay;
            var database = mongoClient.GetDatabase("__empty-db");
            try
            {
                while (true)
                {
                    try
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
                    catch (NotSupportedException) { }

                    await Task.Delay(delay, cancellation);
                    delay = Min(Double(delay), MaxDelay);
                }
            }
            finally
            {
                await mongoClient
                    .DropDatabaseAsync("__empty-db", cancellationToken: default)
                    .ConfigureAwait(false);
            }
        }

        public static void EnsureReplicationSetReady(this IMongoClient mongoClient)
        {
            var delay = InitialDelay;
            var database = mongoClient.GetDatabase("__empty-db");
            try
            {
                while (true)
                {
                    try
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
                    catch (NotSupportedException) { }

                    Thread.Sleep(delay);
                    delay = Min(Double(delay), MaxDelay);
                }
            }
            finally
            {
                mongoClient.DropDatabase("__empty-db");
            }
        }

        private static TimeSpan Min(TimeSpan left, TimeSpan right) => new(Math.Min(left.Ticks, right.Ticks));

        private static TimeSpan Double(TimeSpan timeSpan)
        {
            long ticks;
            try
            {
                ticks = checked(timeSpan.Ticks * 2);
            }
            catch (OverflowException)
            {
                if (timeSpan.Ticks >= 0)
                {
                    return TimeSpan.MaxValue;
                }

                return TimeSpan.MinValue;
            }

            return new TimeSpan(ticks);
        }

        private sealed class Empty
        {
            public int Id { get; set; }
        }
    }
}
