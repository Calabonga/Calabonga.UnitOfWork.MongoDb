namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Extensions fro Queryable LINQ
/// </summary>
public static class PageListExtensions
{
    /// <summary>
    /// Converts the specified source to <see cref="IPagedList{T}"/> by the specified <paramref name="pageIndex"/> and <paramref name="pageSize"/>.
    /// </summary>
    /// <typeparam name="T">The type of the source.</typeparam>
    /// <param name="source">The source to paging.</param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalPages"></param>
    /// <param name="count"></param>
    /// <returns>An instance of the inherited from <see cref="IPagedList{T}"/> interface.</returns>
    public static IPagedList<T> ToPagedList<T>(this IReadOnlyCollection<T> source, int pageIndex, int pageSize, int totalPages, long? count)
    {
        var pagedList = new PagedList<T>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = count ?? 0,
            Items = source,
            TotalPages = totalPages
        };

        return pagedList;
    }
}