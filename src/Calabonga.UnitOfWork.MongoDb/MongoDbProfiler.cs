using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;

namespace Calabonga.UnitOfWork.MongoDb
{
    /// <summary>
    /// Profiler for MongoDb
    /// </summary>
    public class MongoDbProfiler : IDisposable
    {
        private const string profileCollection = "system.profile";
        private const string profileMarker = "command.comment";
        private IRepository _repository;
        private readonly ILogger _logger;
        private bool _isProfiling;
        private bool _disposed;

        public MongoDbProfiler(IRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;

            SetProfiler(true);
        }

        /// <summary>
        /// Включает Profiler для MongoDb
        /// </summary>
        private void SetProfiler(bool isProfiling)
        {
            _isProfiling = isProfiling;
            var profileCommand = new BsonDocument("profile", isProfiling == _isProfiling ? 2 : 0);
            var result = _repository?.Database?.RunCommand<BsonDocument>(profileCommand);
            if (result?.GetValue("ok") == 1)
            {
                return;
            }

            _logger.LogWarning($"Failed to enable profiler for MongoDb from {MethodBase.GetCurrentMethod()?.Name}");
            _isProfiling = false;
        }

        /// <summary>
        /// Записывает найденный запрос в <see cref="ILog"/>. Метод работает при условии, что
        /// включено профилирование <see cref="SetProfiler"/>.
        /// </summary>
        /// <param name="requestId"></param>
        public void LogRequest(string requestId)
        {
            if (!_isProfiling)
            {
                return;
            }

            var collection = _repository.Database.GetCollection<BsonDocument>(profileCollection);
            var doc = collection.Find(new BsonDocument(profileMarker, requestId));
            if (doc == null)
            {
                return;
            }

            var s = doc.FirstOrDefault();
            var command = s.GetValue("command").ToJson();
            var ns = s.GetValue("ns");
            var total = s.GetValue("millis");
            s.TryGetValue("planSummary", out var plan);
            var type = s.GetValue("op");
            var length = s.GetValue("responseLength");
            _logger.LogInformation("[{Namespace}] {Command}, plan: {Plan}, type: {Type}, length: {Length}bytes, duration: {Total}ms",
                ns, command, plan ?? "N/A", type, length, total);
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
                _repository = null;
            }

            _disposed = true;
        }
    }
}