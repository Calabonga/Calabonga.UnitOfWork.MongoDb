namespace Calabonga.UnitOfWork.MongoDb;

public interface IDocument<TType>
{
    TType Id { get; set; }
}

public abstract class DocumentBase<TType> : IDocument<TType>
{
    public TType Id { get; set; }
}