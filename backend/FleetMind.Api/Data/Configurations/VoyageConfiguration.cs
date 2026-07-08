using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class VoyageConfiguration : IEntityTypeConfiguration<Voyage>
    {
        public void Configure(EntityTypeBuilder<Voyage> builder)
        {
            builder.Property(v => v.VoyageNumber)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.HasIndex(v => v.VoyageNumber)
                   .IsUnique();

            builder.HasOne(v => v.OriginPort)
                .WithMany()
                .HasForeignKey(v => v.OriginPortId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.DestinationPort)
                .WithMany()
                .HasForeignKey(v => v.DestinationPortId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(v => v.Status)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(v => v.Notes)
                   .HasMaxLength(2000);

            // Foreign Key Configuration
            builder.HasOne(v => v.Ship)
                   .WithMany(s => s.Voyages)
                   .HasForeignKey(v => v.ShipId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Composite index to optimize listing a ship's voyages in chronological order
            builder.HasIndex(v => new { v.ShipId, v.DepartureDate });

            // Index to optimize filtering voyages by status (e.g. in list views and dashboards)
            builder.HasIndex(v => v.Status);
        }
    }
}
