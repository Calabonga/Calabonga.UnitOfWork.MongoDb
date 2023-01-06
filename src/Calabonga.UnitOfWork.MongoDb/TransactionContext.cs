using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb
{
    public class TransactionContext
    {
        public TransactionContext(
            TransactionOptions transactionOptions,
            IClientSessionHandle? session = null,
            CancellationToken cancellationToken = default)
        {
            TransactionOptions = transactionOptions;
            Session = session;
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; private set; }

        public ILogger<UnitOfWork> Logger { get; private set; }

        public TransactionOptions? TransactionOptions { get; }

        public IClientSessionHandle? Session { get; }

        /// <summary>
        /// Sets instance for Logger from internal UnitOfWork
        /// </summary>
        /// <param name="logger"></param>
        internal void SetLogger(ILogger<UnitOfWork> logger) => Logger = logger;

    }



}
