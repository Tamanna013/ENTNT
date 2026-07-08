using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class ContainerTrackingEventConfiguration : IEntityTypeConfiguration<ContainerTrackingEvent>
    {
        public void Configure(EntityTypeBuilder<ContainerTrackingEvent> builder)
        {
            builder.Property(e => e.EventType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(e => e.Location)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Notes)
                   .HasMaxLength(1000);

            // Container relationship
            builder.HasOne(e => e.Container)
                   .WithMany(c => c.TrackingEvents)
                   .HasForeignKey(e => e.ContainerId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Composite chronological index
            builder.HasIndex(e => new { e.ContainerId, e.Timestamp });
            
            // RecordedByUserId deliberately has NO foreign key constraint configured here,
            // to allow user lifecycle independence as requested.
        }
    }
}
