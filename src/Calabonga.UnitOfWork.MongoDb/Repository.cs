﻿using MongoDB.Bson;
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

            if (mongoDb.GetCollection<BsonDocument>(GetInternalName()) == null)
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
        private string SetDefaultName() => GetInternalName();

        #endregion

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

        #region privates
        private string GetInternalName()
        {
            var name = _collectionNameSelector.GetMongoCollectionName(typeof(TDocument).Name);
            return string.IsNullOrEmpty(name)
                ? throw new UnitOfWorkArgumentNullException($"Cannot read type name from entity in ICllectionNameSelector.GetMongoCollectionName. Argument is NULL: {nameof(name)}")
                : name;
        }

        #endregion
    }
}