using MongoDB.Driver;

namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Extensions fro Queryable LINQ
/// </summary>
public static class QueryablePageListExtensions
{
    /// <summary>
    /// Converts the specified source to <see cref="IPagedList{T}"/> by the specified <paramref name="pageIndex"/> and <paramref name="pageSize"/>.
    /// </summary>
    /// <typeparam name="T">The type of the source.</typeparam>
    /// <param name="source">The source to paging.</param>
    /// <param name="total"></param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <returns>An instance of the inherited from <see cref="IPagedList{T}"/> interface.</returns>
    public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int total, int pageIndex, int pageSize)
    {
        var pagedList = new PagedList<T>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = total,
            Items = source.ToList(),
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };

        return pagedList;
    }
}