using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;
using System.Text;

namespace DairyManagementSystem.TagHelpers
{
    [HtmlTargetElement("paged-pager", Attributes = "list")]
    public class PagedPagerTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public PagedPagerTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public PagedTableState List { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "table-pager");

            var start = List.TotalCount == 0 ? 0 : ((List.Page - 1) * List.PageSize) + 1;
            var end = Math.Min(List.Page * List.PageSize, List.TotalCount);
            var html = new StringBuilder();
            html.Append($"<span class=\"text-muted\">{start}–{end} of {List.TotalCount}</span>");

            if (List.TotalPages <= 1)
            {
                output.Content.SetHtmlContent(html.ToString());
                return;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var action = ViewContext.RouteData.Values["action"]?.ToString() ?? "Index";

            html.Append("<nav><ul class=\"pagination pagination-sm mb-0\">");
            html.Append(PageItem(urlHelper, action, List.Page - 1, "Prev", !List.HasPrevious, active: false));

            for (var p = 1; p <= List.TotalPages; p++)
            {
                if (List.TotalPages > 9 && Math.Abs(p - List.Page) > 3 && p != 1 && p != List.TotalPages)
                {
                    if (p == 2 || p == List.TotalPages - 1)
                    {
                        html.Append("<li class=\"page-item disabled\"><span class=\"page-link\">…</span></li>");
                    }
                    continue;
                }

                html.Append(PageItem(urlHelper, action, p, p.ToString(), disabled: false, active: p == List.Page));
            }

            html.Append(PageItem(urlHelper, action, List.Page + 1, "Next", !List.HasNext, active: false));
            html.Append("</ul></nav>");
            output.Content.SetHtmlContent(html.ToString());
        }

        private string PageItem(IUrlHelper urlHelper, string action, int page, string label, bool disabled, bool active)
        {
            if (disabled)
            {
                return $"<li class=\"page-item disabled\"><span class=\"page-link\">{WebUtility.HtmlEncode(label)}</span></li>";
            }

            var css = active ? "page-item active" : "page-item";
            var href = WebUtility.HtmlEncode(urlHelper.Action(action, Route(page)) ?? "#");
            return $"<li class=\"{css}\"><a class=\"page-link\" href=\"{href}\">{WebUtility.HtmlEncode(label)}</a></li>";
        }

        private Dictionary<string, object?> Route(int page)
        {
            var route = new Dictionary<string, object?>
            {
                ["sort"] = List.Sort,
                ["dir"] = List.Dir,
                ["page"] = page
            };
            foreach (var pair in List.ExtraRoute)
            {
                route[pair.Key] = pair.Value;
            }
            return route;
        }
    }
}
