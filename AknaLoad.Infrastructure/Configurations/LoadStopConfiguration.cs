using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Domain.Configurations
{
    public class LoadStopConfiguration : IEntityTypeConfiguration<LoadStop>
    {
        public void Configure(EntityTypeBuilder<LoadStop> builder)
        {
            // Table Configuration
            builder.HasKey(ls => ls.Id);

            // Foreign Key Relationships
            builder.HasOne(ls => ls.Load)
                .WithMany(l => l.LoadStops)
                .HasForeignKey(ls => ls.LoadId)
                .OnDelete(DeleteBehavior.Cascade);

      
        }
    }
}