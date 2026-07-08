using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Category)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.EntityName)
            .HasMaxLength(50);
    }
}
