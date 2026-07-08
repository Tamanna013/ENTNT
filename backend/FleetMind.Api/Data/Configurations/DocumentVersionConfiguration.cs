using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.Property(dv => dv.ChangeNotes)
            .HasMaxLength(500);

        builder.HasOne(dv => dv.Document)
            .WithMany(d => d.Versions)
            .HasForeignKey(dv => dv.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dv => dv.Attachment)
            .WithMany()
            .HasForeignKey(dv => dv.AttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dv => dv.UploadedByUser)
            .WithMany()
            .HasForeignKey(dv => dv.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(dv => new { dv.DocumentId, dv.VersionNumber })
            .IsUnique();
    }
}
