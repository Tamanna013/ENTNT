using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class ShipConfiguration : IEntityTypeConfiguration<Ship>
    {
        public void Configure(EntityTypeBuilder<Ship> builder)
        {
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(s => s.IMO)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(s => s.IMO)
                .IsUnique();

            builder.Property(s => s.Type)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(s => s.Flag)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.GrossTonnage)
                .HasColumnType("decimal(10,2)");

            builder.HasOne(s => s.Fleet)
                .WithMany(f => f.Ships)
                .HasForeignKey(s => s.FleetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.FleetId);

            builder.HasOne(s => s.PrimaryPhotoAttachment)
                .WithMany()
                .HasForeignKey(s => s.PrimaryPhotoAttachmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
