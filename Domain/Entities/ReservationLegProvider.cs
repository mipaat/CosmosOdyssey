using Domain.Base;

namespace Domain.Entities;

public class ReservationLegProvider : AbstractIdDatabaseEntity
{
    public Reservation? Reservation { get; set; }
    public Guid ReservationId { get; set; }

    public LegProvider? LegProvider { get; set; }
    public Guid LegProviderId { get; set; }
}