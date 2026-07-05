using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Data;

/// <summary>
/// Entity Framework Core database context for the FleetMind AI application.
/// All IEntityTypeConfiguration classes are discovered automatically via ApplyConfigurationsFromAssembly.
/// </summary>
public class FleetMindDbContext : DbContext
{
    public FleetMindDbContext(DbContextOptions<FleetMindDbContext> options)
        : base(options)
    {
    }

    // ─── DbSets ──────────────────────────────────────────────────────────────

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // ─── Model Configuration ─────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discover and apply all IEntityTypeConfiguration<T> classes
        // in this assembly. No need to manually register each one.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetMindDbContext).Assembly);
    }
}
