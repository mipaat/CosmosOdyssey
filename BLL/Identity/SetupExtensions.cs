using BLL.Identity.Options;
using BLL.Identity.Services;
using DAL.EF.DbContexts;
using Utils;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BLL.Identity;

public static class SetupExtensions
{
    public static IServiceCollection AddCustomIdentity(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var cookieSection = configuration.GetRequiredSection(CookieAuthOptions.Section);
        services
            .AddOptions<IdentityOptions>()
            .Configure(options => options.Password.RequiredLength = 16)
            .Bind(configuration.GetSection("Identity"))
            .ValidateDataAnnotations()
            .ValidateOnStart().Services
            .AddOptions<CookieAuthOptions>()
            .Bind(cookieSection)
            .ValidateDataAnnotations().ValidateOnStart();
        var cookieOptions = cookieSection.Get<CookieAuthOptions>()
                            ?? throw new OptionsValidationException(CookieAuthOptions.Section,
                                typeof(CookieAuthOptions),
                                new[] { "Failed to read authentication cookie options" });

        services.AddHttpContextAccessor(); // This is added in .AddIdentity(), but not in .AddIdentityCore(), so adding it manually just in case it doesn't get registered elsewhere.

        services.AddIdentityCore<User>()
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AbstractAppDbContext>()
            .AddDefaultTokenProviders();

        services.TryAddScoped<IRoleValidator<Role>, RoleValidator<Role>>();
        services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<User>>();
        services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<User>>();
        services.TryAddScoped<IUserConfirmation<User>, DefaultUserConfirmation<User>>();

        services.AddScoped<SignInManager<User>>();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies()
            .ApplicationCookie?
            .Configure(o =>
            {
                o.LoginPath = "/Account/Login";
                o.LogoutPath = "/Account/Logout";
                o.SlidingExpiration = cookieOptions.SlidingExpiration;
                o.ExpireTimeSpan = cookieOptions.ExpireTimeSpan;
            });
        services.Configure<SecurityStampValidatorOptions>(o =>
            o.ValidationInterval = cookieOptions.ValidationInterval);

        builder.Services.AddSeeding();

        services.AddAuthorization();

        services.AddIdentityUowAndServices();

        return services;
    }

    private static void AddIdentityUowAndServices(this IServiceCollection services)
    {
        services.AddScoped<IdentityUow>();
        services.AddScoped<UserService>();
    }

    public static void AddSeeding(this IServiceCollection services)
    {
        services.AddOptionsFull<IdentitySeedingOptions>(IdentitySeedingOptions.Section);
    }

    /// <summary>
    /// Seed initial identity data from configuration.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="AddSeeding"/> (called by <see cref="AddCustomIdentity"/>)
    /// to have been called during service registration.
    /// </remarks>
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        var appBuilder = app as IApplicationBuilder;
        await using var scope = appBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
            .CreateAsyncScope();
        var services = scope.ServiceProvider;

        await services.SeedRoles();
        await services.SeedUsers();
    }

    public static void SeedIdentity(this WebApplication app)
    {
        app.SeedIdentityAsync().GetAwaiter().GetResult();
    }

    private static async Task SeedUsers(this IServiceProvider services)
    {
        var options = services.GetService<IOptions<IdentitySeedingOptions>>()?.Value;
        if (options?.Users == null || options.Users.Count == 0) return;

        var userManager = services.GetRequiredService<UserManager<User>>();
        foreach (var userOptions in options.Users)
        {
            var user = await userManager.FindByNameAsync(userOptions.UserName);
            if (user == null)
            {
                user = new User
                {
                    UserName = userOptions.UserName,
                };
                await userManager.CreateAsync(user, userOptions.Password);
            }
            else
            {
                await userManager.AddPasswordAsync(user, userOptions.Password);
            }

            if (userOptions.Roles == null) continue;
            foreach (var roleName in userOptions.Roles)
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }

    private static async Task SeedRoles(this IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        await roleManager.SeedRoles();
    }

    private static async Task SeedRoles(this RoleManager<Role> roleManager)
    {
        foreach (var roleName in RoleNames.All)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null) continue;
            role = new Role
            {
                Name = roleName,
            };
            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Failed to create role {roleName}");
            }
        }
    }
}