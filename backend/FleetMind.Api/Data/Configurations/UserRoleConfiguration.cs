using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the UserRole join entity.
/// Composite primary key on (UserId, RoleId). Cascade delete on join rows
/// when a User or Role is deleted, but the cascade does NOT propagate further
/// (deleting a User removes their UserRole links, not the Roles themselves).
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Composite primary key
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        // User → UserRoles relationship
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Role → UserRoles relationship
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
