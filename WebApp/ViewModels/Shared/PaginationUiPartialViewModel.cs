using BLL.DTO.Entities;
using Utils;

namespace WebApp.ViewModels.Shared;

public class PaginationUiPartialViewModel : IPaginationQuery
{
    public string ControllerName { get; set; } = default!;
    public string ActionName { get; set; } = default!;
    public object? RouteValues { get; set; }
    public int Limit { get; set; }
    public int Page { get; set; }
    public int? Total { get; set; }
    public int AmountOnPage { get; set; }
}