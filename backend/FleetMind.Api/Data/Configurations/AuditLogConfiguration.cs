using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
            builder.Property(a => a.UserName).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Timestamp).IsRequired();

            builder.HasIndex(a => a.EntityName);
            builder.HasIndex(a => a.Timestamp);
            
            // No foreign key constraint enforced on UserId deliberately - keep it a plain nullable Guid column
        }
    }
}
