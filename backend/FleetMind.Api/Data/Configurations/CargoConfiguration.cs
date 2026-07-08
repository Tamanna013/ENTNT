using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class CargoConfiguration : IEntityTypeConfiguration<Cargo>
    {
        public void Configure(EntityTypeBuilder<Cargo> builder)
        {
            builder.Property(c => c.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(c => c.Type)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(c => c.Status)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(c => c.WeightKg)
                   .HasColumnType("decimal(12,2)");

            builder.Property(c => c.DeclaredValue)
                   .HasColumnType("decimal(14,2)");

            builder.Property(c => c.ConsigneeName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.HazardNotes)
                   .HasMaxLength(1000);

            // Foreign Key Configuration
            builder.HasOne(c => c.Voyage)
                   .WithMany(v => v.CargoItems)
                   .HasForeignKey(c => c.VoyageId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.VoyageId);

            // Indexes to optimize filtering by status and type
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.Type);
        }
    }
}
