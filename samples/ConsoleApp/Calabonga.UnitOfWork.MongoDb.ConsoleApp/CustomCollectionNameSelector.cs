using Calabonga.UnitOfWork.MongoDb;

public class CustomCollectionNameSelector : ICollectionNameSelector
{
    public string GetMongoCollectionName(string typeName)
    {
        switch (typeName)
        {
            case "OrderBase":
                return "orders";
        }

        return typeName;
    }
}