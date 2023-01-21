namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Provides the interface(s) for paged list of any type.
/// </summary>
/// <typeparam name="T">The type for paging.</typeparam>
public interface IPagedList<T>
{
    /// <summary>
    /// Gets the page index (current).
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets the total count of the list of type <typeparamref name="T"/>
    /// </summary>
    long TotalCount { get; }

    /// <summary>
    /// Gets the total pages.
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    /// Gets the current page items.
    /// </summary>
    IReadOnlyCollection<T> Items { get; }
}