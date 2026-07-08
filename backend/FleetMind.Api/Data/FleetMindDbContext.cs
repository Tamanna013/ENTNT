using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FleetMind.Api.Common;
using FleetMind.Api.Models;
using FleetMind.Api.Models.Common;
using FleetMind.Api.DTOs.Reporting;
using FleetMind.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Data;

/// <summary>
/// Entity Framework Core database context for the FleetMind AI application.
/// All IEntityTypeConfiguration classes are discovered automatically via ApplyConfigurationsFromAssembly.
/// </summary>
public class FleetMindDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public FleetMindDbContext(
        DbContextOptions<FleetMindDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // ─── DbSets ──────────────────────────────────────────────────────────────

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Fleet> Fleets { get; set; }
    public DbSet<Ship> Ships { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<CrewMember> CrewMembers { get; set; }
    public DbSet<CrewCertification> CrewCertifications { get; set; }
    public DbSet<Voyage> Voyages { get; set; }
    public DbSet<Cargo> Cargo { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<ContainerCargoItem> ContainerCargoItems { get; set; }
    public DbSet<ContainerTrackingEvent> ContainerTrackingEvents { get; set; }
    public DbSet<Port> Ports { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; } = null!;
    public DbSet<FuelLog> FuelLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Incident> Incidents { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;
    public DbSet<AiUsageLog> AiUsageLogs { get; set; } = null!;
    public DbSet<AiConversation> AiConversations { get; set; } = null!;
    public DbSet<AiChatMessage> AiChatMessages { get; set; } = null!;
    public DbSet<UserSettings> UserSettings { get; set; } = null!;

    public DbSet<FleetUtilizationReportRowDto> FleetUtilizationReports { get; set; }
    public DbSet<VoyageManifestReportRowDto> VoyageManifestReports { get; set; }
    public DbSet<FuelEfficiencyReportRowDto> FuelEfficiencyReports { get; set; }

    // ─── Model Configuration ─────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discover and apply all IEntityTypeConfiguration<T> classes
        // in this assembly. No need to manually register each one.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetMindDbContext).Assembly);

        modelBuilder.Entity<FleetUtilizationReportRowDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<VoyageManifestReportRowDto>()
            .HasNoKey()
            .ToView(null);

        modelBuilder.Entity<FuelEfficiencyReportRowDto>()
            .HasNoKey()
            .ToView(null);
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync().GetAwaiter().GetResult();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<AuditLog>();
        var now = DateTime.UtcNow;
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.UserName;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var action = "";
            var changes = new Dictionary<string, object?>();

            if (entry.State == EntityState.Added)
            {
                action = "Created";
            }
            else if (entry.State == EntityState.Deleted)
            {
                action = "Deleted";
            }
            else if (entry.State == EntityState.Modified)
            {
                // Check if this is a soft delete
                var isDeletedProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
                if (isDeletedProperty != null && 
                    isDeletedProperty.OriginalValue is bool oldIsDeleted && !oldIsDeleted &&
                    isDeletedProperty.CurrentValue is bool newIsDeleted && newIsDeleted)
                {
                    action = "Deleted";
                }
                else
                {
                    action = "Updated";
                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified && property.Metadata.Name != "PasswordHash") // Exclude sensitive fields
                        {
                            changes[property.Metadata.Name] = new
                            {
                                old = property.OriginalValue,
                                @new = property.CurrentValue
                            };
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(action))
            {
                auditEntries.Add(new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = entry.Entity.Id.ToString(),
                    Changes = changes.Count > 0 ? JsonSerializer.Serialize(changes) : null,
                    Timestamp = now
                });
            }
        }

        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
