using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGA.Identity.Entities;
using SGA.SharedKernel.Domain.Common;
using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Enums;
using SGA.Desktop.Infrastructure.Config;

namespace SGA.Desktop.Infrastructure.Persistence;

public class DesktopAppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentUserAccessor _currentUser;

    public DesktopAppDbContext(DbContextOptions<DesktopAppDbContext> options, ICurrentUserAccessor currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<Stop> Stops => Set<Stop>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Authorization> Authorizations => Set<Authorization>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<EmployeeProfile> EmployeeProfiles => Set<EmployeeProfile>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Identity tables configured first
        builder.ApplyConfigurationsFromAssembly(typeof(BusConfiguration).Assembly);

        var softDeleteFilterMethod = typeof(DesktopAppDbContext)
            .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!typeof(BaseEntity<Guid>).IsAssignableFrom(clrType) || entityType.IsOwned())
            {
                continue;
            }

            softDeleteFilterMethod.MakeGenericMethod(clrType).Invoke(null, new object[] { builder });
        }
    }

    // Generic per-entity-type global soft-delete filter — same pattern as app-web-mvc's
    // WebAppDbContext, since both apps model the same schema.
    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseEntity<Guid>
    {
        builder.Entity<TEntity>().HasQueryFilter(e => e.RecordStatus == RecordStatus.Active);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var actingUser = _currentUser.UserId ?? "system";
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseEntity<Guid> entity)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAtUtc = now;
                entity.CreatedBy = actingUser;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.ModifiedAtUtc = now;
                entity.ModifiedBy = actingUser;
            }
        }
    }
}
