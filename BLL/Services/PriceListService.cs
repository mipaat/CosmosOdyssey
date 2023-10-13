using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class PriceListService
{
    private readonly AbstractAppDbContext _ctx;

    public PriceListService(AbstractAppDbContext ctx)
    {
        _ctx = ctx;
    }

    /// <summary>
    /// Deletes all expired price lists and their related entities from the database,
    /// but skips the 15 most recent price lists.
    /// NB! Does not require calling DbContext.SaveChanges()
    /// </summary>
    public async Task ExecuteDeleteExpiredPriceLists()
    {
        var query = _ctx.PriceLists
            .Where(e => e.ValidUntil < DateTime.UtcNow)
            .OrderByDescending(e => e.ValidUntil)
            .Skip(15);
        await query.ExecuteDeleteAsync();
    }
}