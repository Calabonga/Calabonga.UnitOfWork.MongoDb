namespace Calabonga.UnitOfWork.MongoDb
{
    /// <summary>
    /// UnitOfWork Argument Null Exception
    /// </summary>
    public class UnitOfWorkArgumentNullException : Exception
    {
        public UnitOfWorkArgumentNullException(string? message) : base(message) { }

        public UnitOfWorkArgumentNullException(string? message, Exception exception) : base(message, exception) { }
    }
}
