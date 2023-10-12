using DAL.EF.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils;

namespace DAL.EF;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbPersistenceEf(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptionsFull<DbLoggingOptions>(DbLoggingOptions.Section);
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AbstractAppDbContext, PostgresAppDbContext>(
            o => o.UseNpgsql(connectionString));
        services.AddScoped<AbstractAppDbContext, PostgresAppDbContext>();
        return services;
    }
}