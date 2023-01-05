using MongoDB.Bson.Serialization.Attributes;

namespace Calabonga.UnitOfWork.MongoDb;

public interface IDocument<TType>
{
    TType Id { get; set; }
}

public abstract class DocumentBase<TType> : IDocument<TType>
{
    [BsonId]
    public TType Id { get; set; } = default!;
}