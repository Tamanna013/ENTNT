using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetMind.Api.Models;

namespace FleetMind.Api.Data.Configurations
{
    public class FuelLogConfiguration : IEntityTypeConfiguration<FuelLog>
    {
        public void Configure(EntityTypeBuilder<FuelLog> builder)
        {
            builder.Property(f => f.FuelType)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(f => f.QuantityLiters)
                .HasColumnType("decimal(12,2)");

            // Deliberately a FINER decimal precision than other cost fields
            // Real-world fuel pricing is commonly quoted to 3-4 decimal places per liter
            builder.Property(f => f.CostPerLiter)
                .HasColumnType("decimal(10,4)");

            builder.Property(f => f.Notes)
                .HasMaxLength(500);

            builder.HasOne(f => f.Ship)
                .WithMany(s => s.FuelLogs)
                .HasForeignKey(f => f.ShipId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Voyage)
                .WithMany(v => v.FuelLogs)
                .HasForeignKey(f => f.VoyageId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(f => f.ShipId);
            builder.HasIndex(f => f.VoyageId);
            builder.HasIndex(f => new { f.ShipId, f.RecordedDate });
        }
    }
}
