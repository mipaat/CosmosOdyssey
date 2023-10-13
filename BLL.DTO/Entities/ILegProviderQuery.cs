using Utils;

namespace BLL.DTO.Entities;

public interface ILegProviderQuery : IPaginationQuery, ISortingQuery
{
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Company { get; set; }
}