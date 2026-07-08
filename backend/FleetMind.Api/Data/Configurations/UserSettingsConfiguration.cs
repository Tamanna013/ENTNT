using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Theme)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.NotificationPreferencesJson);

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Genuine one-to-one relationship enforcing at most one settings row per user
        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
