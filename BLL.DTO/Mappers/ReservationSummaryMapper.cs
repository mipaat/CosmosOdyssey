using BLL.DTO.Entities;
using Domain.Entities;

namespace BLL.DTO.Mappers;

public static class ReservationSummaryMapper
{
    public static IQueryable<ReservationSummary> ProjectToSummary(this IQueryable<Reservation> query) =>
        query.Select(reservation => new ReservationSummary
        {
            Id = reservation.Id,
            FirstName = reservation.FirstName,
            LastName = reservation.LastName,
            CreatedAt = reservation.CreatedAt,
            TotalPrice = reservation.TotalPrice,
            TotalTravelTime = reservation.TotalTravelTime,
            UserId = reservation.UserId,
            LegProviders = reservation.ReservationLegProviders!
                .OrderBy(e => e.LegProvider!.Departure)
                .Select(e => new LegProviderSummary
            {
                Id = e.LegProvider!.Id,
                Price = e.LegProvider!.Price,
                Departure = e.LegProvider!.Departure,
                Arrival = e.LegProvider!.Arrival,
                CompanyName = e.LegProvider!.Company!.Name,
                StartLocation = e.LegProvider!.Leg!.StartLocation!.Name,
                EndLocation = e.LegProvider!.Leg.EndLocation!.Name,
                Distance = e.LegProvider!.Leg.DistanceKm,
                TravelTime = e.LegProvider!.Arrival - e.LegProvider!.Departure,
                ValidUntil = e.LegProvider!.Leg.PriceList!.ValidUntil,
            }).ToList(),
        });
}