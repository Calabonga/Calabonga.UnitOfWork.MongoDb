using Calabonga.UnitOfWork.MongoDb;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplicationWithMongo.Pages.Orders;

[BsonIgnoreExtraElements]
public class Order : DocumentBase<int>
{
    [BsonElement("number")]
    [BsonRepresentation(BsonType.Int32)]
    public int Number { get; set; }

    [BsonElement("title")]
    [BsonRepresentation(BsonType.String)]
    public string Title { get; set; } = default!;

    [BsonElement("description")]
    [BsonRepresentation(BsonType.String)]
    public string? Description { get; set; }

    [BsonElement("items")]
    public ICollection<OrderItem>? Items { get; set; }
}

public class OrderItem : DocumentBase<int>
{
    [BsonElement("name")]
    public string Name { get; set; } = default!;

    [BsonElement("quantity")]
    [BsonRepresentation(BsonType.Int32)]
    public int Quantity { get; set; }

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Double)]
    public double Price { get; set; }
}