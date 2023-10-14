namespace BLL.DTO.Entities;

public class LegProviderSummary
{
    public Guid? Id { get; set; }
    public Guid? LegId { get; set; }
    public decimal Price { get; set; }
    public DateTime Departure { get; set; }
    public DateTime Arrival { get; set; }
    public TimeSpan TravelTime { get; set; }
    public string? CompanyName { get; set; }
    public string StartLocation { get; set; } = default!;
    public string EndLocation { get; set; } = default!;
    public long Distance { get; set; }
    public DateTime ValidUntil { get; set; }

    public List<LegProviderSummary>? SubLegs { get; set; }
}