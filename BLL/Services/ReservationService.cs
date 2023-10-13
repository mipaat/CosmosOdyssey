using System.Security.Claims;
using BLL.DTO.Entities;
using BLL.DTO.Mappers;
using BLL.Identity.Services;
using DAL.EF.DbContexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class ReservationService
{
    private readonly AbstractAppDbContext _ctx;

    public ReservationService(AbstractAppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<ReservationResult> CreateReservation(Guid legProviderId, string firstName, string lastName,
        ClaimsPrincipal user)
    {
        var legProvider = await _ctx.LegProviders.Where(e => e.Id == legProviderId).ProjectToSummary()
            .SingleOrDefaultAsync();
        if (legProvider == null)
        {
            return EReservationResultType.NotFound;
        }

        if (legProvider.ValidUntil <
            DateTime.UtcNow
                .AddSeconds(2)) // Making the validity check a bit more strict than it needs to be, just in case
        {
            return EReservationResultType.Expired;
        }

        var reservation = new Reservation
        {
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = GetTotalPrice(new[] { legProvider }),
            TotalTravelTime = GetTotalTravelTime(new[] { legProvider }),
            UserId = user.GetUserId(),
            ReservationLegProviders = new List<ReservationLegProvider>(),
        };

        reservation.ReservationLegProviders.Add(new ReservationLegProvider
        {
            LegProviderId = legProviderId,
            ReservationId = reservation.Id,
        });

        _ctx.Add(reservation);

        return reservation.Id;
    }

    public static decimal GetTotalPrice(IEnumerable<LegProviderSummary> summaries) => summaries.Sum(e => e.Price);

    public static TimeSpan GetTotalTravelTime(ICollection<LegProviderSummary> summaries) =>
        summaries.Max(e => e.Arrival) - summaries.Min(e => e.Departure);
}