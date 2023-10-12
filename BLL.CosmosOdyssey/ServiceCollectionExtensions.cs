using BLL.CosmosOdyssey.BackgroundServices;
using BLL.CosmosOdyssey.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.CosmosOdyssey;

public static class ServiceCollectionExtensions
{
    public static void AddCosmosOdyssey(this IServiceCollection services)
    {
        services.AddScoped<PriceListService>();
        services.AddHostedService<PriceListFetcherBackgroundService>();
    }
}