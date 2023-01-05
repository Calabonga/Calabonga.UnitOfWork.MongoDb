namespace WebApplicationWithMongo.Application.TagHelpers.PagedListHelper;

public class PagerData
{
    public int GroupIndex { get; set; }

    public int MinPage { get; set; }

    public int MaxPage { get; set; }

    public int NextPage { get; set; }

    public int PreviousPage { get; set; }

    public int TotalPages { get; set; }

    public int PageIndex { get; set; }
}

public abstract class PagerPageBase
{
    protected PagerPageBase(string title, int value, bool isActive = false, bool isDisabled = false)
    {
        Title = title;
        Value = value;
        IsActive = isActive;
        IsDisabled = isDisabled;
    }

    public string Title { get; }

    public int Value { get; }

    public bool IsActive { get; }

    public bool IsDisabled { get; }

}

public class PagerPageDisabled : PagerPageBase
{
    public PagerPageDisabled(string title, int value) : base(title, value, false, true)
    {
    }
}

public class PagerPageActive : PagerPageBase
{
    public PagerPageActive(string title, int value) : base(title, value, true)
    {
    }
}

public class PagerPage : PagerPageBase
{
    public PagerPage(string title, int value) : base(title, value)
    {
    }
}