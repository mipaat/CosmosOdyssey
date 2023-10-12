using Domain.Base;

namespace Domain.Entities;

public class LegProvider : AbstractExternalDatabaseEntity
{
    public decimal Price { get; set; }
    public DateTime Departure { get; set; }
    public DateTime Arrival { get; set; }

    public Leg? Leg { get; set; }
    public Guid LegId { get; set; }

    public Company? Company { get; set; }
    public Guid CompanyId { get; set; }
}