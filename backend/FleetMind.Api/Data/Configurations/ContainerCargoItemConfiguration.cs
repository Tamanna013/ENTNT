using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMind.Api.Data.Configurations
{
    public class ContainerCargoItemConfiguration : IEntityTypeConfiguration<ContainerCargoItem>
    {
        public void Configure(EntityTypeBuilder<ContainerCargoItem> builder)
        {
            // Composite primary key
            builder.HasKey(cci => new { cci.ContainerId, cci.CargoId });

            // Configure foreign key relationships
            builder.HasOne(cci => cci.Container)
                   .WithMany(c => c.ContainerCargoItems)
                   .HasForeignKey(cci => cci.ContainerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cci => cci.Cargo)
                   .WithMany() // The prompt didn't specify ICollection<ContainerCargoItem> on Cargo, so WithMany() is empty for unidirectional navigation
                   .HasForeignKey(cci => cci.CargoId)
                   // We use Cascade as requested. SQL Server might reject it due to multiple cascade paths.
                   // If migration fails, this will be changed to NoAction / ClientCascade
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
