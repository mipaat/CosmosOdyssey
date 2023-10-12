using Domain.Base;

namespace BLL.DTO.Entities;

public class LegProviderSummary : AbstractIdDatabaseEntity
{
    public decimal Price { get; set; }
    public DateTime Departure { get; set; }
    public DateTime Arrival { get; set; }
    public TimeSpan TravelTime { get; set; }
    public string CompanyName { get; set; } = default!;
    public string StartLocation { get; set; } = default!;
    public string EndLocation { get; set; } = default!;
    public long Distance { get; set; }
}