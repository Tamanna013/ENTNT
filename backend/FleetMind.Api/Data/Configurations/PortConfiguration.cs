using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class PortConfiguration : IEntityTypeConfiguration<Port>
    {
        public void Configure(EntityTypeBuilder<Port> builder)
        {
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.UnLocode)
                .IsRequired()
                .HasMaxLength(10);

            // Real-world UN/LOCODEs are unique
            builder.HasIndex(p => p.UnLocode)
                .IsUnique();

            builder.Property(p => p.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.City)
                .IsRequired()
                .HasMaxLength(150);

            // Precision is deliberately set to hold standard GPS coordinate precision
            builder.Property(p => p.Latitude)
                .HasColumnType("decimal(9,6)");

            builder.Property(p => p.Longitude)
                .HasColumnType("decimal(9,6)");
        }
    }
}
