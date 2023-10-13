namespace WebApp.ViewModels.Shared;

public record SortHeaderModel(
    string Name,
    Func<BLL.DTO.Entities.SortOptions, Dictionary<string, string>> QueryStringFunc,
    BLL.DTO.Entities.SortOptions CurrentSort,
    string? DisplayName = null
);