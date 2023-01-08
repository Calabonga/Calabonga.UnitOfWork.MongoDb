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
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of the inherited from <see cref="IPagedList{T}"/> interface.</returns>
    public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IAsyncCursor<T> source, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var list = await source.ToListAsync(cancellationToken);

        var items = list
            .Skip((pageIndex) * pageSize)
            .Take(pageSize).ToList();

        var pagedList = new PagedList<T>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = list.Count,
            Items = items,
            TotalPages = (int)Math.Ceiling(list.Count / (double)pageSize)
        };

        return pagedList;
    }
}