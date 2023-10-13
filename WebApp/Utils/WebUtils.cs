using System.Dynamic;
using System.Net;
using BLL.DTO.Entities;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Utils;

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

    public static IHtmlContent GetSortingFormSection(ISortingQuery query)
    {
        var builder = new HtmlContentBuilder()
            .AppendHtml(HiddenFor(nameof(query.SortBy) + '.' + nameof(query.SortBy.Name),
                query.SortBy.Name ?? string.Empty))
            .AppendHtml(HiddenFor(nameof(query.SortBy) + '.' + nameof(query.SortBy.Descending),
                query.SortBy.Descending?.ToString() ?? string.Empty));
        return builder;
    }

    private static HtmlString HiddenFor(string name, string value)
    {
        return new HtmlString(
            $"<input type=\"hidden\" name=\"{WebUtility.HtmlEncode(name)}\" value=\"{WebUtility.HtmlEncode(value)}\">");
    }

    // Copied from https://levelup.gitconnected.com/using-asp-net-mvc-to-specify-which-element-in-a-navigation-bar-is-active-9c3dac154f9c
    public static string ActiveClass(this IHtmlHelper htmlHelper, string? controllers = null, string? actions = null,
        string cssClass = "active")
    {
        if (controllers == null && actions == null)
        {
            return string.Empty;
        }

        var currentController = htmlHelper.ViewContext.RouteData.Values["controller"] as string;
        var currentAction = htmlHelper.ViewContext.RouteData.Values["action"] as string;

        var acceptedControllers = (controllers ?? currentController ?? string.Empty).Split(',');
        var acceptedActions = (actions ?? currentAction ?? string.Empty).Split(',');

        return acceptedControllers.Contains(currentController) &&
               (actions == null || acceptedActions.Contains(currentAction))
            ? cssClass
            : string.Empty;
    }
}