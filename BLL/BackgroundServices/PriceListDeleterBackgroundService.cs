using BLL.Base;
using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.BackgroundServices;

public class PriceListDeleterBackgroundService : BaseLockedTimedBackgroundService
{
    public PriceListDeleterBackgroundService(ILogger<PriceListDeleterBackgroundService> logger,
        IServiceProvider services) : base(logger, services, TimeSpan.FromHours(1))
    {
    }

    protected override async Task DoLockedWork(object? state)
    {
        await using var scope = Services.CreateAsyncScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AbstractAppDbContext>();
        await ctx.PriceLists
            .Where(e => e.ValidUntil < DateTime.UtcNow)
            .OrderByDescending(e => e.ValidUntil)
            .Skip(15)
            .ExecuteDeleteAsync();
        await ctx.Locations
            .Where(location => !ctx.Legs
                .Any(leg => leg.StartLocationId == location.Id || leg.EndLocationId == location.Id))
            .ExecuteDeleteAsync();
        await ctx.Companies
            .Where(c => !ctx.LegProviders
                .Any(lp => lp.CompanyId == c.Id))
            .ExecuteDeleteAsync();
        await ctx.Reservations.Where(r => !ctx.ReservationLegProviders
                .Any(rlp => rlp.ReservationId == r.Id))
            .ExecuteDeleteAsync();
    }
}