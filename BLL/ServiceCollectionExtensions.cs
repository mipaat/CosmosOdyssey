using BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class ServiceCollectionExtensions
{
    public static void AddBll(this IServiceCollection services)
    {
        services.AddScoped<RouteService>();
        services.AddScoped<ReservationService>();
    }
}