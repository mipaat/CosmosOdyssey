using BLL.Base;
using BLL.Services;
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
        await scope.ServiceProvider.GetRequiredService<PriceListService>().ExecuteDeleteExpiredPriceLists();
    }
}