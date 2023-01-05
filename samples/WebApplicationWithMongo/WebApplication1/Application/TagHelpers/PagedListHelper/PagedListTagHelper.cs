using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebApplicationWithMongo.Application.TagHelpers.PagedListHelper
{
    /// <summary>
    /// PagedListLite TagHelper
    /// </summary>
    [HtmlTargetElement("pager", Attributes = ControllerAttributeName)]
    [HtmlTargetElement("pager", Attributes = PagerListPageAttributeName)]
    [HtmlTargetElement("pager", Attributes = FragmentAttributeName)]
    [HtmlTargetElement("pager", Attributes = PagerListParamsAttributeName)]
    [HtmlTargetElement("pager", Attributes = HostAttributeName)]
    [HtmlTargetElement("pager", Attributes = ProtocolAttributeName)]
    [HtmlTargetElement("pager", Attributes = PagerListPageSizeAttributeName)]
    [HtmlTargetElement("pager", Attributes = PagerListPageIndexAttributeName)]
    [HtmlTargetElement("pager", Attributes = PagerListTotalCountAttributeName)]
    public class PagedListTagHelper : TagHelper
    {
        private readonly IPagedListTagHelperService _pagedListTagHelperService;

        private const string PagerListPageSizeAttributeName = "asp-paged-list-page-size";
        private const string PagerListPageIndexAttributeName = "asp-paged-list-page-index";
        private const string PagerListTotalCountAttributeName = "asp-paged-list-total-pages";
        private const string PagerListPageAttributeName = "asp-paged-list-page";
        private const string PagerListParamsAttributeName = "asp-paged-list-action";
        private const string HostAttributeName = "asp-host";
        private const string FragmentAttributeName = "asp-fragment";
        private const string PagedListRouteAttributeName = "asp-route-parameter";
        private const string PagedListRouteDataAttributeName = "asp-route-data";
        private const string ProtocolAttributeName = "asp-protocol";
        private const string ControllerAttributeName = "asp-controller";

        private readonly IDictionary<string, string> _routeValues = new Dictionary<string, string>();

        public PagedListTagHelper(IHtmlGenerator generator, IPagedListTagHelperService pagedListTagHelperService)
        {
            _pagedListTagHelperService = pagedListTagHelperService;
            Generator = generator;
        }

        #region Properties

        protected IHtmlGenerator Generator { get; }

        [HtmlAttributeName(PagerListPageSizeAttributeName)]
        public int PagedListSize { get; set; }

        [HtmlAttributeName(PagerListPageIndexAttributeName)]
        public int PagedListIndex { get; set; }

        [HtmlAttributeName(PagerListTotalCountAttributeName)]
        public int PagedListTotalCount { get; set; }

        [HtmlAttributeName(PagerListPageAttributeName)]
        public string? PageName { get; set; }

        [HtmlAttributeName(PagedListRouteAttributeName)]
        public string? RouteParameter { get; set; }

        [HtmlAttributeName(PagedListRouteDataAttributeName)]
        public object? RouteParameters { get; set; }

        /// <summary>
        /// The URL fragment name.
        /// </summary>
        [HtmlAttributeName(FragmentAttributeName)]
        public string? Fragment { get; set; }

        [ViewContext] public ViewContext ViewContext { get; set; } = default!;

        /// <summary>
        /// The protocol for the URL, such as &quot;http&quot; or &quot;https&quot;.
        /// </summary>
        [HtmlAttributeName(ProtocolAttributeName)]
        public string? Protocol { get; set; }

        /// <summary>
        /// The host name.
        /// </summary>
        [HtmlAttributeName(HostAttributeName)]
        public string? Host { get; set; }

        /// <summary>
        /// The name of the controller.
        /// </summary>
        /// <remarks>Must be <c>null</c> if <see cref="Route"/> is non-<c>null</c>.</remarks>
        [HtmlAttributeName(ControllerAttributeName)]
        public string? Controller { get; set; }

        #endregion

        /// <summary>
        /// Synchronously executes the <see cref="T:Microsoft.AspNetCore.Razor.TagHelpers.TagHelper" /> with the given <paramref name="context" /> and
        /// <paramref name="output" />.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (PagedListTotalCount < 1)
            {
                return;
            }

            var pager = _pagedListTagHelperService.Calculate(PagedListIndex, VisibleGroupCount, PagedListSize, PagedListTotalCount);

            #region -------------- begin render pager -----------------

            var ul = new TagBuilder("ul");
            ul.AddCssClass(RootTagCss);

            #endregion


            // pages
            var pages = _pagedListTagHelperService.GeneratePages(pager);
            foreach (var page in pages)
            {
                var li = new TagBuilder("li");
                li.AddCssClass(PageItemCss);
                if (page.IsActive)
                {
                    li.AddCssClass(ActiveTagCss);
                }

                if (page.IsDisabled)
                {
                    li.AddCssClass(DisableCss);
                }

                li.InnerHtml.AppendHtml(GenerateLink(page.Title, page.Value.ToString()));
                ul.InnerHtml.AppendHtml(li);

            }

            output.Content.AppendHtml(ul);
            base.Process(context, output);
        }

        private string DisableCss => "disabled";

        private string PageLinkCss => "page-link";

        private string RootTagCss => "pagination";

        private string ActiveTagCss => "active";

        private string PageItemCss => "page-item";

        private byte VisibleGroupCount => 10;

        /// <summary>
        /// Generate TagBuilder for link
        /// </summary>
        /// <param name="linkText"></param>
        /// <param name="routeValue"></param>
        /// <returns></returns>
        private TagBuilder GenerateLink(string linkText, string routeValue)
        {
            RouteValueDictionary routeValues;
            routeValues = RouteParameter != null ? new RouteValueDictionary(_routeValues) { { RouteParameter, routeValue } } : new RouteValueDictionary(_routeValues);

            if (RouteParameters == null)
            {
                return Generator.GeneratePageLink(
                    viewContext: ViewContext,
                    pageHandler: "",
                    pageName: PageName,
                    routeValues: routeValues,
                    hostname: Host,
                    linkText: linkText,
                    fragment: Fragment,
                    htmlAttributes: new { @class = PageLinkCss },
                    protocol: Protocol);
            }

            var values = RouteParameters.GetType().GetProperties();
            if (values.Any())
            {
                foreach (var propertyInfo in values)
                {
                    routeValues.Add(propertyInfo.Name, propertyInfo.GetValue(RouteParameters));
                }
            }

            return Generator.GeneratePageLink(
                viewContext: ViewContext,
                pageHandler: "",
                pageName: PageName,
                routeValues: routeValues,
                hostname: Host,
                linkText: linkText,
                fragment: Fragment,
                htmlAttributes: new { @class = PageLinkCss },
                protocol: Protocol);

        }
    }
}