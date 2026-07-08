using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class ContainerConfiguration : IEntityTypeConfiguration<Container>
    {
        public void Configure(EntityTypeBuilder<Container> builder)
        {
            builder.Property(c => c.ContainerNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(c => c.ContainerNumber)
                   .IsUnique();

            builder.Property(c => c.Type)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.Property(c => c.Status)
                   .IsRequired()
                   .HasMaxLength(30);

            builder.HasOne(c => c.CurrentVoyage)
                   .WithMany() // No inverse nav required based on the specs
                   .HasForeignKey(c => c.CurrentVoyageId)
                   .OnDelete(DeleteBehavior.SetNull);
            // Required foreign key (not applicable here directly but keep existing structure)

            // Index to optimize filtering containers by status
            builder.HasIndex(c => c.Status);
        }
    }
}
