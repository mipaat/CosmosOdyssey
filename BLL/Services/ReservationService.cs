using System.Security.Claims;
using BLL.DTO.Entities;
using BLL.DTO.Mappers;
using BLL.Identity.Services;
using DAL.EF.DbContexts;
using DAL.EF.Extensions;
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

    public async Task<AccessResult<ReservationSummary>> GetByIdForUser(Guid reservationId, Guid userId)
    {
        var result = await _ctx.Reservations
            .Where(e => e.Id == reservationId)
            .ProjectToSummary()
            .FirstOrDefaultAsync();
        if (result == null)
        {
            return EAccessResultType.NotFound;
        }

        if (result.UserId != userId)
        {
            return EAccessResultType.NotAllowed;
        }

        return result;
    }

    public Task<List<ReservationSummary>> GetAllForUser(Guid userId, IReservationQuery search)
    {
        IQueryable<Reservation> query = _ctx.Reservations;
        query = query.Where(e => e.UserId == userId);

        SortBehaviour<Reservation> sortBehaviour = search.SortBy.Name switch
        {
            nameof(ReservationSummary.FirstName) => (e => e.FirstName, false),
            nameof(ReservationSummary.LastName) => (e => e.LastName, false),
            nameof(ReservationSummary.CreatedAt) => (e => e.CreatedAt, false),
            nameof(ReservationSummary.TotalPrice) => (e => e.TotalPrice, false),
            nameof(ReservationSummary.TotalTravelTime) => (e => e.TotalTravelTime, false),
            _ => (e => e.CreatedAt, false),
        };

        query = query.OrderBy(search.SortBy, sortBehaviour);

        query = query.Paginate(search);

        return query.ProjectToSummary().ToListAsync();
    }

    public async Task<ReservationResult> CreateReservation(List<Guid> legProviderIds, string firstName, string lastName,
        ClaimsPrincipal user)
    {
        if (legProviderIds.Count == 0)
        {
            throw new ArgumentException("At least one ID is required", nameof(legProviderIds));
        }

        var legProviders = await _ctx.LegProviders
            .Where(e => legProviderIds.Contains(e.Id))
            .ProjectToSummary()
            .ToListAsync();
        if (legProviders.Count < legProviderIds.Count)
        {
            return EReservationResultType.NotFound;
        }

        foreach (var legProvider in legProviders)
        {
            if (legProvider.ValidUntil < DateTime.UtcNow
                    .AddSeconds(2)) // Making the validity check a bit more strict than it needs to be, just in case
            {
                return EReservationResultType.Expired;
            }
        }

        var reservation = new Reservation
        {
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = GetTotalPrice(legProviders),
            TotalTravelTime = GetTotalTravelTime(legProviders),
            UserId = user.GetUserId(),
            ReservationLegProviders = new List<ReservationLegProvider>(),
        };

        foreach (var legProviderId in legProviderIds)
        {
            reservation.ReservationLegProviders.Add(new ReservationLegProvider
            {
                LegProviderId = legProviderId,
                ReservationId = reservation.Id,
            });
        }

        _ctx.Add(reservation);

        return reservation.Id;
    }

    public static decimal GetTotalPrice(IEnumerable<LegProviderSummary> summaries) => summaries.Sum(e => e.Price);

    public static TimeSpan GetTotalTravelTime(ICollection<LegProviderSummary> summaries) =>
        summaries.Max(e => e.Arrival) - summaries.Min(e => e.Departure);
}