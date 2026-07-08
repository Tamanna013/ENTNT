using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(a => a.StoredFileName)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(a => a.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(a => new { a.EntityName, a.EntityId });
        }
    }
}
