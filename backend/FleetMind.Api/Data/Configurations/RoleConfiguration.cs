using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(200);

        builder.Property(r => r.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired();
    }
}
