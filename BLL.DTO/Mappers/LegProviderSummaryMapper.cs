using BLL.DTO.Entities;
using Domain.Entities;

namespace BLL.DTO.Mappers;

public static class LegProviderSummaryMapper
{
    public static IQueryable<LegProviderSummary> ProjectToSummary(this IQueryable<LegProvider> q) =>
        q.Select(legProvider => new LegProviderSummary
        {
            Id = legProvider.Id,
            Price = legProvider.Price,
            Departure = legProvider.Departure,
            Arrival = legProvider.Arrival,
            CompanyName = legProvider.Company!.Name,
            StartLocation = legProvider.Leg!.StartLocation!.Name,
            EndLocation = legProvider.Leg.EndLocation!.Name,
            Distance = legProvider.Leg.DistanceKm,
            TravelTime = legProvider.Arrival - legProvider.Departure,
            ValidUntil = legProvider.Leg.PriceList!.ValidUntil,
        });
}