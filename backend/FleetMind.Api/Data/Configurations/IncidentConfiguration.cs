using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
    {
        public void Configure(EntityTypeBuilder<Incident> builder)
        {
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(i => i.Severity)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(i => i.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue(IncidentStatus.Reported);

            builder.Property(i => i.ResolutionNotes)
                .HasMaxLength(2000);

            // ShipId - required, restrict delete
            builder.HasOne(i => i.Ship)
                .WithMany(s => s.Incidents)
                .HasForeignKey(i => i.ShipId)
                .OnDelete(DeleteBehavior.Restrict);

            // VoyageId - nullable, set null
            builder.HasOne(i => i.Voyage)
                .WithMany(v => v.Incidents)
                .HasForeignKey(i => i.VoyageId)
                .OnDelete(DeleteBehavior.SetNull);

            // ReportedByUserId - REQUIRED, Restrict delete
            builder.HasOne(i => i.ReportedByUser)
                .WithMany()
                .HasForeignKey(i => i.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Composite index on (Severity, Status)
            builder.HasIndex(i => new { i.Severity, i.Status });
        }
    }
}
