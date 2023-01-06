using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb
{
    public sealed class Repository<TDocument, TType> : IRepository<TDocument, TType>
        where TDocument : DocumentBase<TType>
    {
        private readonly string _entityName;
        private readonly ICollectionNameSelector _collectionNameSelector;

        public Repository(IDatabaseBuilder databaseBuilder)
        {
            _collectionNameSelector = databaseBuilder.CollectionNameSelector;
            _entityName = SetDefaultName();
            Collection = GetOrCreateCollection(databaseBuilder);
        }

        #region base protected

        /// <summary>
        /// Коллекция документов
        /// </summary>
        public IMongoCollection<TDocument> Collection { get; }

        /// <summary>
        /// Возвращает коллекцию другого типа
        /// </summary>
        /// <typeparam name="T">тип коллекции</typeparam>
        /// <param name="name">наименование коллекции в MongoDb</param>
        /// <returns>коллекция MongoDb</returns>
        private IMongoCollection<T> GetCollection<T>(string name) =>
            Collection.Database.GetCollection<T>(name)
                .WithWriteConcern(WriteConcern.WMajority)
                .WithReadConcern(ReadConcern.Local)
                .WithReadPreference(ReadPreference.Primary);

        /// <summary>
        /// Returns collection of items (getting already exists or create before and return)
        /// </summary>
        /// <param name="databaseBuilder"></param>
        /// <returns>возвращает коллекцию MongoDb</returns>
        private IMongoCollection<TDocument> GetOrCreateCollection(IDatabaseBuilder databaseBuilder)
        {
            var mongoDb = databaseBuilder.Build();

            if (mongoDb.GetCollection<BsonDocument>(_collectionNameSelector.GetMongoCollectionName(typeof(TDocument).Name)) == null)
            {
                mongoDb.CreateCollection(_entityName);
            }

            return mongoDb.GetCollection<TDocument>(_entityName)
                .WithWriteConcern(WriteConcern.WMajority)
                .WithReadConcern(ReadConcern.Local)
                .WithReadPreference(ReadPreference.Primary);
        }


        /// <summary>
        /// Sets default name for entity name of the repository
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string SetDefaultName()
        {
            var entityName = _collectionNameSelector.GetMongoCollectionName(typeof(TDocument).Name);
            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentNullException(nameof(entityName));
            }

            return _collectionNameSelector.GetMongoCollectionName(typeof(TDocument).Name);
        }

        #endregion

        public CollectionNamespace CollectionNamespace => Collection.CollectionNamespace;
        public IMongoDatabase Database => Collection.Database;
        public IBsonSerializer<TDocument> DocumentSerializer => Collection.DocumentSerializer;
        public IMongoIndexManager<TDocument> Indexes => Collection.Indexes;
        public MongoCollectionSettings Settings => Collection.Settings;

        /// <summary>
        /// Returns paged collection of the items
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="options"></param>
        /// <param name="pageIndex"></param>
        public async Task<IPagedList<TDocument>> GetPagedAsync
        (
            int pageIndex,
            int pageSize,
            FilterDefinition<TDocument> filter,
            CancellationToken cancellationToken,
            FindOptions<TDocument>? options = null)
        {
            var itemsFind = await Collection.FindAsync(filter, options, cancellationToken);
            return await itemsFind.ToPagedListAsync(pageIndex, pageSize, cancellationToken);
        }

        #region GetSession

        /// <summary>
        /// Создает транзакцию и возвращает ее
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>транзакцию для Write-операций</returns>
        private IClientSessionHandle GetSession(CancellationToken cancellationToken = default, ClientSessionOptions? options = null)
            => Collection.Database.Client.StartSession(options, cancellationToken);

        /// <summary>
        /// Создает транзакцию и возвращает ее
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>транзакцию для Write-операций</returns>
        private Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken = default, ClientSessionOptions? options = null)
            => Collection.Database.Client.StartSessionAsync(options, cancellationToken);

        #endregion
    }
}