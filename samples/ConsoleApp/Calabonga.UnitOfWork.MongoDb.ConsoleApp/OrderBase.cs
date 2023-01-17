using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Calabonga.UnitOfWork.MongoDb.ConsoleApp;

[BsonIgnoreExtraElements]
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

    [BsonElement("lastCenter")]
    [BsonIgnoreIfNull]
    public string? LastNumber { get; set; }
    
    [BsonIgnoreIfNull]
    [BsonElement("centers")]
    public string[]? Numbers { get; set; }

    protected abstract OrderType OrderType { get; }

    public override string ToString()
    {
        return Numbers != null
            ? $"[{OrderType}] {CreatedAt:G} {Title} {Description} {LastNumber} {State}  ({string.Join(",", Numbers)})"
            : $"[{OrderType}] {CreatedAt:G} {Title} {Description} {LastNumber} {State} ";
    }
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