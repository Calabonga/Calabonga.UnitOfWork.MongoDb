using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Calabonga.UnitOfWork.MongoDb.ConsoleApp;

[BsonIgnoreExtraElements]
[MetadataType(typeof(DocumentBaseMetadata<int>))]
[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(OrderInternal), typeof(OrderExternal))]
public abstract class OrderBase : DocumentBase<int>
{
    [BsonElement("title")]
    public string Title { get; set; } = default!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("state")]
    [BsonRepresentation(BsonType.String)]
    public DocumentState State { get; set; }

    protected abstract OrderType OrderType { get; }

    public override string ToString() => $"[{OrderType}] {CreatedAt:G} {Title} {Description} {State}";
}

[BsonDiscriminator("Internal")]
public class OrderInternal : OrderBase
{
    protected override OrderType OrderType => OrderType.Internal;
}

[BsonDiscriminator("External")]
public class OrderExternal : OrderBase
{
    protected override OrderType OrderType => OrderType.External;
}

public class DocumentBaseMetadata<TType>
{
    [BsonId] public TType Id { get; set; } = default!;
}