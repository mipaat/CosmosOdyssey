using Domain.Base;

namespace BLL.DTO.Entities;

public class ReservationSummary : AbstractIdDatabaseEntity
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public decimal TotalPrice { get; set; }
    public TimeSpan TotalTravelTime { get; set; }
    public Guid UserId { get; set; }
    public List<LegProviderSummary> LegProviders { get; set; } = default!;
}