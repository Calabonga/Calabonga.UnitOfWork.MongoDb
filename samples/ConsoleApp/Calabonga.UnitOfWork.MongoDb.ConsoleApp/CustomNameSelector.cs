using Calabonga.UnitOfWork.MongoDb;
using Calabonga.UnitOfWork.MongoDb.ConsoleApp;

public class CustomNameSelector : ICollectionNameSelector
{
    public string GetMongoCollectionName(string typeName)
    {
        switch (typeName)
        {
            case nameof(OrderBase):
                return "orders";

            default:
                throw new InvalidOperationException();
        }
    }
}