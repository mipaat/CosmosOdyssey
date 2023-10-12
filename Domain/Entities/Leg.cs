using Domain.Base;

namespace Domain.Entities;

public class Leg : AbstractExternalDatabaseEntity
{
    public Guid RouteInfoExternalId { get; set; }
    public long DistanceKm { get; set; }

    public PriceList? PriceList { get; set; }
    public Guid PriceListId { get; set; }

    public Location? StartLocation { get; set; }
    public Guid StartLocationId { get; set; }
    public Location? EndLocation { get; set; }
    public Guid EndLocationId { get; set; }

    public ICollection<LegProvider>? LegProviders { get; set; }
}