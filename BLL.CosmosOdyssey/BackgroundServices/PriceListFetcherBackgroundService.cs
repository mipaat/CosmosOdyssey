using BLL.Base;
using BLL.CosmosOdyssey.Services;
using DAL.EF.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.CosmosOdyssey.BackgroundServices;

public class PriceListFetcherBackgroundService : BaseLockedTimedBackgroundService
{
    public PriceListFetcherBackgroundService(ILogger<PriceListFetcherBackgroundService> logger, IServiceProvider services) :
        base(logger, services, TimeSpan.FromMinutes(1))
    {
    }

    private static readonly TimeSpan StartRetryDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromMinutes(4);
    private const int RetryDelayMultiplier = 2;
    
    private TimeSpan? _retryDelay;

    protected override async Task DoLockedWork(object? _)
    {
        await using var scope = Services.CreateAsyncScope();
        var priceListFetcherService = scope.ServiceProvider.GetRequiredService<PriceListService>();
        DTO.PriceList priceList;
        try
        {
            priceList = await PriceListService.FetchPriceList();
        }
        catch (Exception e)
        {
            _retryDelay ??= StartRetryDelay;
            _retryDelay = _retryDelay.Value * RetryDelayMultiplier;
            if (_retryDelay > MaxRetryDelay)
            {
                _retryDelay = MaxRetryDelay;
            }
            Logger.LogError(e, "Failed to fetch price list, retrying in {RetryDelay}", _retryDelay);
            Timer?.Change(_retryDelay.Value, MaxRetryDelay);
            return;
        }

        await priceListFetcherService.AddPriceList(priceList);
        await scope.ServiceProvider.GetRequiredService<AbstractAppDbContext>().SaveChangesAsync();

        Timer?.Change(TimeSpan.FromTicks((priceList.ValidUntil - DateTime.UtcNow).Ticks), MaxRetryDelay);
        _retryDelay = null;
    }
}