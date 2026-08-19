using DairyManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DairyManagementSystem.TagHelpers
{
    [HtmlTargetElement("sort-th", Attributes = "column,list")]
    public class SortThTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public SortThTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public string Column { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public PagedTableState List { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "th";
            output.TagMode = TagMode.StartTagAndEndTag;

            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var action = ViewContext.RouteData.Values["action"]?.ToString() ?? "Index";
            var isCurrent = List.IsSortedBy(Column);
            var nextDir = isCurrent && List.Dir == "asc" ? "desc" : "asc";

            var route = new Dictionary<string, object?>
            {
                ["sort"] = Column,
                ["dir"] = nextDir,
                ["page"] = 1
            };
            foreach (var pair in List.ExtraRoute)
            {
                route[pair.Key] = pair.Value;
            }

            var href = urlHelper.Action(action, route) ?? "#";
            var indicator = isCurrent ? (List.Dir == "desc" ? " ↓" : " ↑") : "";
            var css = isCurrent ? "sort-link is-sorted" : "sort-link";

            output.Content.SetHtmlContent(
                $"<a class=\"{css}\" href=\"{href}\">{System.Net.WebUtility.HtmlEncode(Title)}<span class=\"sort-ind\">{indicator}</span></a>");
        }
    }
}
