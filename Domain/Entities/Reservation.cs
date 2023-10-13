using Domain.Base;
using Domain.Entities.Identity;

namespace Domain.Entities;

public class Reservation : AbstractIdDatabaseEntity
{
    // Note: I would usually avoid storing names like this, because it doesn't account for cultural differences, middle names etc.
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public decimal TotalPrice { get; set; }
    public TimeSpan TotalTravelTime { get; set; }

    public User? User { get; set; }
    public Guid UserId { get; set; }

    public ICollection<ReservationLegProvider>? ReservationLegProviders { get; set; }
}