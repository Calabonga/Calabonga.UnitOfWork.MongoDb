    /// <summary>
    /// // Calabonga: update summary (2023-03-09 04:14 MongoDbProfiler)
    /// </summary>
    public class MongoDbProfiler : IDisposable
    {
        private const string profileCollection = "system.profile";
        private const string profileMarker = "command.comment";
        private IMongoDbContext? context;
        private readonly ILog log;
        private bool _isProfiling;
        private bool _disposed;

        public MongoDbProfiler(IMongoDbContext context, ILog log)
        {
            this.context = context;
            this.log = log.ForContext<MongoDbProfiler>();
            SetProfiler(true);
        }

        /// <summary>
        /// Включает Profiler для MongoDb
        /// </summary>
        private void SetProfiler(bool isProfiling)
        {
            _isProfiling = isProfiling;
            var profileCommand = new BsonDocument("profile", isProfiling == _isProfiling ? 2 : 0);
            var result = ((MongoDbContext)context!)?.Database?.RunCommand<BsonDocument>(profileCommand);
            if (result?.GetValue("ok") != 1)
            {
                throw new DocsiFailedToEnableMongoDbProfilerException();
            }
        }

        /// <summary>
        /// Записывает найденный запрос в <see cref="ILog"/>. Метод работает при условии, что
        /// включено профилирование <see cref="SetProfiler"/>.
        /// </summary>
        /// <param name="requestId"></param>
        public void LogRequest(string requestId)
        {
            var collection = context?.GetCollection<BsonDocument>(profileCollection, ReadPreference.Primary);
            var doc = collection.Find(new BsonDocument(profileMarker, requestId));
            if (doc == null)
            {
                return;
            }

            var s = doc.FirstOrDefault();
            var command = s.GetValue("command").ToJson();
            var db = s.GetValue("ns");
            var total = s.GetValue("millis");
            s.TryGetValue("planSummary", out var plan);
            var type = s.GetValue("op");
            var length = s.GetValue("responseLength");
            log.Warn($"[{db}] {command}, plan:{plan}, type: {type}, length: {length}, duration: {total}ms");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                SetProfiler(false);
                context = null;
            }

            _disposed = true;
        }
    }
