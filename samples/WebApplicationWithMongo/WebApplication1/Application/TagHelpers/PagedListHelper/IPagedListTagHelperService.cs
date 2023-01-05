namespace WebApplicationWithMongo.Application.TagHelpers.PagedListHelper;

public interface IPagedListTagHelperService
{
    PagerData Calculate(int pageIndex, int itemsInPager, int pageSize, int totalCount);

    List<PagerPageBase> GeneratePages(PagerData pager);
}