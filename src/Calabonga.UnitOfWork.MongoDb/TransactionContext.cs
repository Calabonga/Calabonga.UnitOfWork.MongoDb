using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb
{
    /// <summary>
    /// Propagates context between operations that executing in transaction
    /// </summary>
    public sealed class TransactionContext
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

        /// <summary>Propagates notification that operations should be canceled.</summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// ILogger for intern operation logging
        /// </summary>
        public ILogger<UnitOfWork> Logger { get; private set; }

        /// <summary>Transaction options.</summary>
        public TransactionOptions? TransactionOptions { get; }

        /// <summary>
        /// A handle to an underlying reference counted IClientSession.
        /// </summary>
        /// <seealso cref="T:MongoDB.Driver.IClientSession" />
        public IClientSessionHandle? Session { get; }

        /// <summary>
        /// Sets instance for Logger from internal UnitOfWork
        /// </summary>
        /// <param name="logger"></param>
        internal void SetLogger(ILogger<UnitOfWork> logger) => Logger = logger;

        /// <summary>
        /// Returns default TransactionContext with default settings.
        /// TransactionOptions as new object. Session is null. CancellationToken.None
        /// </summary>
        public static TransactionContext Default => new(new TransactionOptions(), null, CancellationToken.None);
    }
}
