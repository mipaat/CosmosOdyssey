using BLL.DTO.Entities;

namespace WebApp.ViewModels.Route;

public class SearchModel : ILegProviderQuery
{
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Company { get; set; }

    public SortOptions SortBy { get; set; } = new()
    {
        Name = nameof(LegProviderSummary.Price),
        Descending = false,
    };

    public List<LegProviderSummary>? Results { get; set; }

    public Dictionary<string, string> ToQueryStringValues(SortOptions sortOptions) => new()
    {
        {nameof(From), From ?? string.Empty},
        {nameof(To), To ?? string.Empty},
        {nameof(Company), Company ?? string.Empty},
        {$"{nameof(SortBy)}.{nameof(SortBy.Name)}", sortOptions.Name ?? string.Empty},
        {$"{nameof(SortBy)}.{nameof(SortBy.Descending)}", sortOptions.Descending?.ToString() ?? string.Empty},
    };
}