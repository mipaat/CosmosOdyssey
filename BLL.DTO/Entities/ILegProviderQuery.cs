namespace BLL.DTO.Entities;

public interface ILegProviderQuery
{
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Company { get; set; }

    public SortOptions SortBy { get; set; }
}