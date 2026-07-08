using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations;

public class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.Content)
            .IsRequired();
            
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
