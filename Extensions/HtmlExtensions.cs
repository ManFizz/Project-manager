using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MegaProject.Extensions;

public static class HtmlExtensions
{
    public static IHtmlContent SortLink(
        this IHtmlHelper html,
        string text,
        string column,
        string currentColumn,
        string currentDirection,
        object routeValues)
    {
        var http = html.ViewContext.HttpContext;

        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers
            .ParseQuery(http.Request.QueryString.ToString())
            .ToDictionary(x => x.Key, x => x.Value.ToString());

        var direction = (currentColumn == column && currentDirection == "asc")
            ? "desc"
            : "asc";

        query["SortColumn"] = column;
        query["SortDirection"] = direction;

        var url = http.Request.Path + "?" +
                  string.Join("&", query.Select(q =>
                      $"{q.Key}={Uri.EscapeDataString(q.Value)}"));

        var icon = currentColumn == column
            ? (currentDirection == "asc" ? " ↑" : " ↓")
            : "";

        var tag = new TagBuilder("a");
        tag.Attributes["href"] = url;
        tag.AddCssClass("text-decoration-none");
        tag.InnerHtml.Append(text + icon);

        return tag;
    }
}