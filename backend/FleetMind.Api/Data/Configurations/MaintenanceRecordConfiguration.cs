using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetMind.Api.Models;

namespace FleetMind.Api.Data.Configurations;

public class MaintenanceRecordConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.Property(m => m.Type)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(m => m.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.EstimatedCost)
            .HasColumnType("decimal(12,2)");

        builder.Property(m => m.ActualCost)
            .HasColumnType("decimal(12,2)");

        builder.Property(m => m.PerformedBy)
            .HasMaxLength(200);

        builder.HasOne(m => m.Ship)
            .WithMany(s => s.MaintenanceRecords)
            .HasForeignKey(m => m.ShipId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite index to support both "this ship's maintenance history" AND "what's due soon across the fleet"
        builder.HasIndex(m => new { m.ShipId, m.ScheduledDate });
    }
}
