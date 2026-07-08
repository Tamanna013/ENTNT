using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class FleetConfiguration : IEntityTypeConfiguration<Fleet>
    {
        public void Configure(EntityTypeBuilder<Fleet> builder)
        {
            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(f => f.Name)
                .IsUnique();

            builder.HasOne(f => f.HomePort)
                .WithMany()
                .HasForeignKey(f => f.HomePortId)
                .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

            builder.Property(f => f.Description)
                .HasMaxLength(1000);

            builder.Property(f => f.Status)
                .IsRequired()
                .HasMaxLength(30);
        }
    }
}
