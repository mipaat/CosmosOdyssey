using System.Linq.Expressions;
using BLL.DTO.Entities;
using BLL.DTO.Mappers;
using DAL.EF.DbContexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Utils;

namespace BLL.Services;

public class RouteService
{
    private readonly AbstractAppDbContext _ctx;

    public RouteService(AbstractAppDbContext ctx)
    {
        _ctx = ctx;
    }

    public Task<List<LegProviderSummary>> Search(ILegProviderQuery queryParams)
    {
        IQueryable<LegProvider> query = _ctx.LegProviders;

        query = query.Where(e => e.Leg!.PriceList!.ValidUntil > DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(queryParams.From))
        {
            var pattern = PostgresUtils.GetContainsPattern(queryParams.From);
            query = query.Where(e =>
                EF.Functions.ILike(e.Leg!.StartLocation!.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.To))
        {
            var pattern = PostgresUtils.GetContainsPattern(queryParams.To);
            query = query.Where(e =>
                EF.Functions.ILike(e.Leg!.EndLocation!.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(queryParams.Company))
        {
            var pattern = PostgresUtils.GetContainsPattern(queryParams.Company);
            query = query.Where(e =>
                EF.Functions.ILike(e.Company!.Name, pattern));
        }

        (Expression<Func<LegProvider, dynamic>> orderExpression, bool descending) sortOptions =
            queryParams.SortBy.Name switch
            {
                nameof(LegProviderSummary.Price) => (e => e.Price, false),
                nameof(LegProviderSummary.Distance) => (e => e.Leg!.DistanceKm, false),
                nameof(LegProviderSummary.TravelTime) => (e => e.Arrival - e.Departure, false),
                _ => (e => e.Arrival - e.Departure, false),
            };

        if (queryParams.SortBy.Descending != null)
        {
            sortOptions.descending = queryParams.SortBy.Descending.Value;
        }

        query = sortOptions.descending
            ? query.OrderByDescending(sortOptions.orderExpression)
            : query.OrderBy(sortOptions.orderExpression);

        return query.ProjectToSummary().ToListAsync();
    }
}