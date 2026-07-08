using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetMind.Api.Models;

namespace FleetMind.Api.Data.Configurations
{
    public class CrewMemberConfiguration : IEntityTypeConfiguration<CrewMember>
    {
        public void Configure(EntityTypeBuilder<CrewMember> builder)
        {
            builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.Rank).IsRequired().HasMaxLength(30);
            builder.Property(c => c.Status).IsRequired().HasMaxLength(30);
            builder.Property(c => c.Nationality).IsRequired().HasMaxLength(100);
            
            builder.Property(c => c.LicenseNumber).IsRequired().HasMaxLength(50);
            builder.HasIndex(c => c.LicenseNumber).IsUnique();

            builder.Property(c => c.ContactEmail).HasMaxLength(256);
            builder.Property(c => c.ContactPhone).HasMaxLength(30);

            builder.HasOne(c => c.Ship)
                .WithMany(s => s.CrewMembers)
                .HasForeignKey(c => c.ShipId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(c => c.ShipId);
        }
    }
}
