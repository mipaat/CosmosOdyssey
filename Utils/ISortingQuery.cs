using BLL.DTO.Entities;

namespace Utils;

public interface ISortingQuery
{
    public SortOptions SortBy { get; set; }
}