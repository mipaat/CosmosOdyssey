using Domain.Base;

namespace Domain.Entities;

public class Location : AbstractExternalDatabaseEntity
{
    public string Name { get; set; } = default!;

    public ICollection<Leg>? OutgoingLegs { get; set; }
    public ICollection<Leg>? IncomingLegs { get; set; }
}