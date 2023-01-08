namespace Calabonga.UnitOfWork.MongoDb;

/// <summary>
/// Provides the implementation of the <see cref="IPagedList{T}"/> and converter.
/// </summary>
/// <typeparam name="TSource">The type of the source.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
internal class PagedList<TSource, TResult> : IPagedList<TResult>
{
    /// <summary>
    /// Gets the index of the page.
    /// </summary>
    /// <value>The index of the page.</value>
    public int PageIndex { get; }

    /// <summary>
    /// Gets the size of the page.
    /// </summary>
    /// <value>The size of the page.</value>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total count.
    /// </summary>
    /// <value>The total count.</value>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the total pages.
    /// </summary>
    /// <value>The total pages.</value>
    public int TotalPages { get; }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>The items.</value>
    public IList<TResult> Items { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{TSource, TResult}" /> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="converter">The converter.</param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="indexFrom">The index from.</param>
    public PagedList(IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter,
        int pageIndex, int pageSize, int indexFrom)
    {
        if (indexFrom > pageIndex)
        {
            throw new UnitOfWorkArgumentNullException(
                $"indexFrom: {indexFrom} > pageIndex: {pageIndex}, must indexFrom <= pageIndex");
        }

        if (source is IQueryable<TSource> querable)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = querable.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            var items = querable.Skip(PageIndex * PageSize).Take(PageSize).ToArray();

            Items = new List<TResult>(converter(items));
        }
        else
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            var enumerable = source as TSource[] ?? source.ToArray();
            TotalCount = enumerable.Length;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            var items = enumerable.Skip(PageIndex * PageSize).Take(PageSize).ToArray();
            Items = new List<TResult>(converter(items));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{TSource, TResult}" /> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="converter">The converter.</param>
    public PagedList(IPagedList<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
    {
        PageIndex = source.PageIndex;
        PageSize = source.PageSize;
        TotalCount = source.TotalCount;
        TotalPages = source.TotalPages;

        Items = new List<TResult>(converter(source.Items));
    }
}

/// <summary>
/// Represents the default implementation of the <see cref="IPagedList{T}"/> interface.
/// </summary>
/// <typeparam name="T">The type of the data to page</typeparam>
public class PagedList<T> : IPagedList<T>
{
    /// <summary>
    /// Gets or sets the index of the page.
    /// </summary>
    /// <value>The index of the page.</value>
    public int PageIndex { get; init; }

    /// <summary>
    /// Gets or sets the size of the page.
    /// </summary>
    /// <value>The size of the page.</value>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    /// <value>The total count.</value>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets or sets the total pages.
    /// </summary>
    /// <value>The total pages.</value>
    public int TotalPages { get; init; }

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    /// <value>The items.</value>
    public IList<T> Items { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{T}" /> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="pageSize">The size of the page.</param>
    internal PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
    {
        if (source is IQueryable<T> queryable)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = queryable.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            Items = queryable.Skip((PageIndex) * PageSize).Take(PageSize).ToList();
        }
        else
        {
            var enumerable = source.ToList();
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = enumerable.Count;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            Items = enumerable
                .Skip((PageIndex) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{T}" /> class.
    /// </summary>
    internal PagedList() => Items = Array.Empty<T>();
}