using Domain.Base;

namespace Domain.Entities;

public class PriceList : AbstractExternalDatabaseEntity
{
    public DateTime ValidUntil { get; set; }

    public ICollection<Leg>? Legs { get; set; }
}