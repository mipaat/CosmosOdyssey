using BLL.DTO.Entities;
using BLL.DTO.Mappers;
using DAL.EF.DbContexts;
using DAL.EF.Extensions;
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

    public Task<List<LegProviderSummary>> GetLegProvidersByIds(params Guid[] ids)
    {
        return _ctx.LegProviders.Where(e => ids.Contains(e.Id)).ProjectToSummary().ToListAsync();
    }

    public Task<List<LegProviderSummary>> Search(ILegProviderQuery queryParams)
    {
        IQueryable<LegProvider> query = _ctx.LegProviders;

        query = query.WhereValid();

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

        SortBehaviour<LegProvider> sortOptions =
            queryParams.SortBy.Name switch
            {
                nameof(LegProviderSummary.Price) => (e => e.Price, false),
                nameof(LegProviderSummary.Distance) => (e => e.Leg!.DistanceKm, false),
                nameof(LegProviderSummary.TravelTime) => (e => e.Arrival - e.Departure, false),
                nameof(LegProviderSummary.Departure) => (e => e.Departure, false),
                nameof(LegProviderSummary.Arrival) => (e => e.Arrival, false),
                _ => (e => e.Arrival - e.Departure, false),
            };

        query = query.OrderBy(queryParams.SortBy, sortOptions);

        query = query.Paginate(queryParams);

        return query.ProjectToSummary().ToListAsync();
    }
}