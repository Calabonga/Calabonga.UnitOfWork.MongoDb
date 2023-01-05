namespace WebApplicationWithMongo.Application.TagHelpers.PagedListHelper;

public class PagedListTagHelperService : IPagedListTagHelperService
{
    private readonly string _previous;
    private readonly string _next;
    private readonly string _last;
    private readonly string _first;

    public PagedListTagHelperService()
    {
        _first = "«";
        _previous = "‹";
        _next = "›";
        _last = "»";
        //_first    = "‹‹‹";
        // _next     = "";
        // _previous = "«";
        //_last     = "›››";
        //_first    = "◄◄";
        //_previous = "◄";
        //_next     = "►";
        //_last     = "►►";
    }

    public PagerData Calculate(int pageIndex, int itemsInPager, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var groupIndex = (int)Math.Floor(Convert.ToDecimal(pageIndex) / Convert.ToDecimal(itemsInPager));
        var minPage = groupIndex * itemsInPager + 1;
        var maxPage = minPage + itemsInPager - 1;
        var prevPage = minPage - 1;
        var nextPage = maxPage + 1;

        if (minPage <= 1)
        {
            prevPage = 1;
        }

        if (maxPage > totalPages)
        {
            maxPage = totalPages;
        }

        if (nextPage > totalPages)
        {
            nextPage = 0;
        }

        return new PagerData
        {
            PageIndex = pageIndex,
            TotalPages = totalPages,
            GroupIndex = groupIndex,
            MinPage = minPage,
            MaxPage = maxPage,
            NextPage = nextPage,
            PreviousPage = prevPage
        };
    }

    public List<PagerPageBase> GeneratePages(PagerData pager)
    {
        var list = new List<PagerPageBase>();
        var firstAndPrev = PreviousPages(pager);
        var pages = GetNumberPages(pager);
        var nextAndLast = NextPages(pager);
        list.AddRange(firstAndPrev);
        list.AddRange(pages);
        list.AddRange(nextAndLast);
        return list;
    }

    private IEnumerable<PagerPageBase> PreviousPages(PagerData pager)
    {
        yield return pager.PageIndex == 0
            ? new PagerPageDisabled(_first, 1)
            : new PagerPage(_first, 1);

        yield return pager.PreviousPage > 1
            ? new PagerPage(_previous, pager.MinPage - 1)
            : new PagerPageDisabled(_previous, pager.MinPage - 1);
    }

    private static IEnumerable<PagerPageBase> GetNumberPages(PagerData pager)
    {
        for (var i = pager.MinPage; i <= pager.MaxPage; i++)
        {
            if (i == pager.PageIndex + 1)
            {
                yield return new PagerPageActive(i.ToString(), i);
                continue;
            }
            yield return new PagerPage(i.ToString(), i);
        }
    }

    private IEnumerable<PagerPageBase> NextPages(PagerData pager)
    {
        yield return pager.NextPage >= pager.MaxPage
            ? new PagerPage(_next, pager.MaxPage + 1)
            : new PagerPageDisabled(_next, pager.MaxPage);

        yield return pager.PageIndex + 1 == pager.TotalPages
            ? new PagerPageDisabled(_last, pager.TotalPages)
            : new PagerPage(_last, pager.TotalPages);
    }
}