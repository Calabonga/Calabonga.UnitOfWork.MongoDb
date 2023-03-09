using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb
{
    /// <summary>
    /// Generic repository for wrapper of mongoDb Collection
    /// </summary>
    /// <typeparam name="TDocument">The type of the entity.</typeparam>
    /// <typeparam name="TType"></typeparam>
    public sealed class Repository<TDocument, TType> : IRepository<TDocument, TType>
        where TDocument : DocumentBase<TType>
    {
        private readonly ILogger<UnitOfWork> _logger;
        private const string _dataFacetName = "dataFacet";
        private const string _countFacetName = "countFacet";

        private readonly string _entityName;
        private readonly ICollectionNameSelector _collectionNameSelector;

        /// <summary>
        /// Returns instance of new <see cref="IRepository{TDocument,TType}"/>
        /// </summary>
        /// <param name="databaseBuilder"></param>
        /// <param name="logger"></param>
        /// <param name="writeConcern">default is WriteConcern.WMajority</param>
        /// <param name="readConcern">default is ReadConcern.Local</param>
        /// <param name="readPreference">default is ReadPreference.Primary</param>
        public Repository(IDatabaseBuilder databaseBuilder,
            ILogger<UnitOfWork> logger,
            WriteConcern? writeConcern = null,
            ReadConcern? readConcern = null,
            ReadPreference? readPreference = null)
        {
            _logger = logger;
            _collectionNameSelector = databaseBuilder.CollectionNameSelector;
            _entityName = SetDefaultName();
            Collection = GetOrCreateCollection(databaseBuilder, writeConcern, readConcern, readPreference);
        }

        /// <summary>
        /// MongoDb collection (<see cref="IMongoCollection{TDocument}"/>
        /// </summary>
        public IMongoCollection<TDocument> Collection { get; }

        /// <summary>Gets the namespace of the collection.</summary>
        public CollectionNamespace CollectionNamespace => Collection.CollectionNamespace;

        /// <summary>Gets the database.</summary>
        public IMongoDatabase Database => Collection.Database;

        /// <summary>Gets the document serializer.</summary>
        public IBsonSerializer<TDocument> DocumentSerializer => Collection.DocumentSerializer;

        /// <summary>Gets the index manager.</summary>
        public IMongoIndexManager<TDocument> Indexes => Collection.Indexes;

        /// <summary>Gets the settings.</summary>
        public MongoCollectionSettings Settings => Collection.Settings;

        /// <summary> Returns paged collection of the items </summary>
        /// <remarks>Pagination for MongoDB is using AggregateFacet</remarks>
        /// <param name="pageSize"></param>
        /// <param name="filter"></param>
        /// <param name="sorting"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="pageIndex"></param>
        public async Task<IPagedList<TDocument>> GetPagedAsync
        (
            int pageIndex,
            int pageSize,
            FilterDefinition<TDocument> filter,
            SortDefinition<TDocument> sorting,
            CancellationToken cancellationToken)
        {
            var countFacet = AggregateFacet.Create(_countFacetName,
                PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
                {
                    PipelineStageDefinitionBuilder.Count<TDocument>()
                }));

            var dataFacet = AggregateFacet.Create(_dataFacetName,
                PipelineDefinition<TDocument, TDocument>.Create(new[]
                {
                    PipelineStageDefinitionBuilder.Sort(sorting),
                    PipelineStageDefinitionBuilder.Skip<TDocument>((pageIndex - 1) * pageSize),
                    PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize),
                }));


            var aggregation = await Collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync(cancellationToken: cancellationToken);

            var count = aggregation.First()
                .Facets.First(x => x.Name == _countFacetName)
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count;

            var totalPages = (int)Math.Ceiling((double)count! / pageSize);

            var data = aggregation.First()
                .Facets.First(x => x.Name == _dataFacetName)
                .Output<TDocument>();

            return data.ToPagedList(pageIndex, pageSize, totalPages, count);
        }

        #region privates

        #region GetSession

        /// <summary>
        /// Creates and return <see cref="IClientSessionHandle"/> (session object)
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        private IClientSessionHandle GetSession(CancellationToken cancellationToken = default, ClientSessionOptions? options = null)
            => Collection.Database.Client.StartSession(options, cancellationToken);

        /// <summary>
        /// Creates and return <see cref="IClientSessionHandle"/> (session object)
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        private Task<IClientSessionHandle> GetSessionAsync(CancellationToken cancellationToken = default, ClientSessionOptions? options = null)
            => Collection.Database.Client.StartSessionAsync(options, cancellationToken);

        #endregion

        private string GetInternalName()
        {
            var name = _collectionNameSelector.GetMongoCollectionName(typeof(TDocument).Name);
            return string.IsNullOrEmpty(name)
                ? throw new UnitOfWorkArgumentNullException($"Cannot read type name from entity in ICllectionNameSelector.GetMongoCollectionName. Argument is NULL: {nameof(name)}")
                : name;
        }

        /// <summary>
        /// Returns new instance of <see cref="IMongoCollection{TDocument}"/>
        /// </summary>
        /// <typeparam name="T">type of collection</typeparam>
        /// <param name="name">collection name in MongoDb</param>
        /// <param name="writeConcern">default is WriteConcern.WMajority</param>
        /// <param name="readConcern">default is ReadConcern.Local</param>
        /// <param name="readPreference">default is ReadPreference.Primary</param>
        /// <returns>MongoDb collection</returns>
        private IMongoCollection<T> GetCollection<T>(string name,
            WriteConcern? writeConcern = null,
            ReadConcern? readConcern = null,
            ReadPreference? readPreference = null)
            => Collection.Database.GetCollection<T>(name)
                .WithWriteConcern(writeConcern ?? WriteConcern.WMajority)
                .WithReadConcern(readConcern ?? ReadConcern.Local)
                .WithReadPreference(readPreference ?? ReadPreference.Primary);

        /// <summary>
        /// Returns collection of items (getting already exists or create before and return)
        /// </summary>
        /// <param name="databaseBuilder"></param>
        /// <param name="writeConcern">default is WriteConcern.WMajority</param>
        /// <param name="readConcern">default is ReadConcern.Local</param>
        /// <param name="readPreference">default is ReadPreference.Primary</param>
        /// <returns>MongoDb collection</returns>
        private IMongoCollection<TDocument> GetOrCreateCollection(IDatabaseBuilder databaseBuilder,
            WriteConcern? writeConcern = null,
            ReadConcern? readConcern = null,
            ReadPreference? readPreference = null)
        {
            var mongoDb = databaseBuilder.Build();
            if (mongoDb.GetCollection<BsonDocument>(GetInternalName()) == null)
            {
                mongoDb.CreateCollection(_entityName);
            }

            return mongoDb.GetCollection<TDocument>(_entityName)
                .WithWriteConcern(writeConcern ?? WriteConcern.WMajority)
                .WithReadConcern(readConcern ?? ReadConcern.Local)
                .WithReadPreference(readPreference ?? ReadPreference.Primary);
        }


        /// <summary>
        /// Sets default name for entity name of the repository
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string SetDefaultName() => GetInternalName();

        #endregion
    }
}