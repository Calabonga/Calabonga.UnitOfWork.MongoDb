namespace Calabonga.UnitOfWork.MongoDb
{
    // Calabonga: update summary (2023-01-02 05:06 ICollectionNameSelector)
    public interface ICollectionNameSelector
    {
        /// <summary>
        /// // Calabonga: update summary (2023-01-02 05:06 ICollectionNameSelector)
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        string GetMongoCollectionName(string typeName);
    }
}
