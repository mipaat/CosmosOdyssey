using System.Dynamic;
using System.Net;
using BLL.DTO.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.WebUtilities;

namespace WebApp.Utils;

public static class WebUtils
{
    private static IHtmlContent SpanFor(DateTime? value, string culture = "en-US")
    {
        return new HtmlString($"<span class='date-time-local' culture='{culture}'>{value.ToString()} UTC</span>");
    }

    public static IHtmlContent ToSpan(this DateTime value, HttpContext context) =>
        SpanFor(value, context.CurrentCultureName());

    public static string CurrentCultureName(this HttpContext context) =>
        context.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture.Name ?? "en-US";
    
    public static object GetRouteValues(this HttpRequest request)
    {
        var queryValues = QueryHelpers.ParseQuery(request.QueryString.ToString());

        dynamic customObject = new ExpandoObject();
        
        foreach (var query in queryValues)
        {
            ((IDictionary<string, object>)customObject)[query.Key] = query.Value;
        }

        return customObject;
    }

    public static object GetRouteValues(this HttpContext context) => context.Request.GetRouteValues();

    public static IHtmlContent GetPaginationFormSection(IPaginationQuery query)
    {
        var builder = new HtmlContentBuilder()
            .AppendHtml(HiddenFor(nameof(query.Page), query.Page.ToString()))
            .AppendHtml(HiddenFor(nameof(query.Limit), query.Limit.ToString()));
        return builder;
    }

    private static HtmlString HiddenFor(string name, string value)
    {
        return new HtmlString(
            $"<input type=\"hidden\" name=\"{WebUtility.HtmlEncode(name)}\" value=\"{WebUtility.HtmlEncode(value)}\">");
    }
}