using Domain.Entities;

namespace DAL.EF.Extensions;

public static class LegProviderExtensions
{
    public static IQueryable<LegProvider> WhereValid(this IQueryable<LegProvider> query) =>
        query.Where(e => e.Leg!.PriceList!.ValidUntil > DateTime.UtcNow);
}