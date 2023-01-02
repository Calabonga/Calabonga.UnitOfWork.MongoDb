namespace Calabonga.UnitOfWork.MongoDb.ConsoleApp
{
    public class Document : DocumentBase<int>
    {
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DocumentState State { get; set; }

        public override string ToString() => $"{CreatedAt:G} {Title} {Description} {State}";
    }
}
