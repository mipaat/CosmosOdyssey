using BLL.DTO.Entities;

namespace WebApp.ViewModels.Reservations;

public class IndexModel : IReservationQuery
{
    public List<ReservationSummary> Reservations { get; set; } = default!;

    public int Page { get; set; }
    public int Limit { get; set; } = 50;
    public int? Total { get; set; }

    public SortOptions SortBy { get; set; } = new()
    {
        Name = nameof(ReservationSummary.CreatedAt),
        Descending = false,
    };

    public Dictionary<string, string> ToQueryStringValues(SortOptions sortOptions) => new()
    {
        {$"{nameof(SortBy)}.{nameof(SortBy.Name)}", sortOptions.Name ?? string.Empty},
        {$"{nameof(SortBy)}.{nameof(SortBy.Descending)}", sortOptions.Descending?.ToString() ?? string.Empty},
        {nameof(Page), Page.ToString()},
        {nameof(Limit), Limit.ToString()},
    };
}