using MongoDB.Bson.Serialization.Attributes;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Base object for Document inheritance that have BsonId property.
/// Required for some base operations
/// </summary>
/// <typeparam name="TType"></typeparam>
public abstract class DocumentBase<TType>
{
    /// <summary>
    /// Identifier Bson Document
    /// </summary>
    [BsonId]
    public TType Id { get; set; } = default!;
}