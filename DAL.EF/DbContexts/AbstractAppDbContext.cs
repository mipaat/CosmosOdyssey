using DAL.EF.Converters;
using Domain.Entities;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DAL.EF.DbContexts;

public class AbstractAppDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole,
    UserLogin, RoleClaim, UserToken>
{
    public DbSet<Company> Companies { get; set; } = default!;
    public DbSet<Leg> Legs { get; set; } = default!;
    public DbSet<LegProvider> LegProviders { get; set; } = default!;
    public DbSet<Location> Locations { get; set; } = default!;
    public DbSet<PriceList> PriceLists { get; set; } = default!;

    public DbSet<Reservation> Reservations { get; set; } = default!;
    public DbSet<ReservationLegProvider> ReservationLegProviders { get; set; } = default!;

    private readonly ILoggerFactory? _loggerFactory;
    private readonly DbLoggingOptions? _dbLoggingOptions;

    public AbstractAppDbContext(DbContextOptions options,
        IOptions<DbLoggingOptions> dbLoggingOptions,
        ILoggerFactory? loggerFactory = null) : base(options)
    {
        _loggerFactory = loggerFactory;
        _dbLoggingOptions = dbLoggingOptions.Value;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseLoggerFactory(_loggerFactory);
        if (_dbLoggingOptions?.SensitiveDataLogging ?? false)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ReconfigureIdentity();

        builder.Entity<Leg>()
            .HasOne<Location>(e => e.StartLocation)
            .WithMany(e => e.OutgoingLegs)
            .HasForeignKey(e => e.StartLocationId);
        builder.Entity<Leg>()
            .HasOne<Location>(e => e.EndLocation)
            .WithMany(e => e.IncomingLegs)
            .HasForeignKey(e => e.EndLocationId);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<DateTimeUtcConverter>();
    }
}

internal static class DbContextConfigurationExtensions
{
    public static void ReconfigureIdentity(this ModelBuilder builder)
    {
        builder.Entity<UserRole>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserRole>()
            .HasOne(e => e.Role)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserClaim>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserClaims)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RoleClaim>()
            .HasOne(e => e.Role)
            .WithMany(e => e.RoleClaims)
            .HasForeignKey(e => e.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLogin>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserLogins)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserToken>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserTokens)
            .HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}