using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FleetMind.Api.Models;

namespace FleetMind.Api.Data.Configurations
{
    public class CrewCertificationConfiguration : IEntityTypeConfiguration<CrewCertification>
    {
        public void Configure(EntityTypeBuilder<CrewCertification> builder)
        {
            builder.Property(c => c.CertificationName).IsRequired().HasMaxLength(150);

            // CrewMemberId: Cascade - deleting a crew record's certifications along with it is correct
            builder.HasOne(c => c.CrewMember)
                .WithMany(cm => cm.Certifications)
                .HasForeignKey(c => c.CrewMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttachmentId: Restrict - a certification record should never silently lose its file reference
            builder.HasOne(c => c.Attachment)
                .WithMany()
                .HasForeignKey(c => c.AttachmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
