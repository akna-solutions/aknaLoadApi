using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Domain.Configurations
{
    public class LoadConfiguration : IEntityTypeConfiguration<Load>
    {
        public void Configure(EntityTypeBuilder<Load> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.CompanyId)
                .IsRequired();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.LoadCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.LoadCode)
                .IsUnique();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.LoadType)
                .IsRequired()
                .HasConversion<int>();

          
            // DateTime fields (Legacy)
            builder.Property(x => x.PickupDateTime);

            builder.Property(x => x.DeliveryDeadline);

            // New multi-stop time fields
            builder.Property(x => x.EarliestPickupTime);

            builder.Property(x => x.LatestDeliveryTime);

            builder.Property(x => x.PublishedAt);

            builder.Property(x => x.MatchedAt);

            builder.Property(x => x.CompletedAt);

            // Decimal fields with precision
            builder.Property(x => x.Weight)
                .HasPrecision(10, 2);

            builder.Property(x => x.Volume)
                .HasPrecision(10, 3);

            builder.Property(x => x.FixedPrice)
                .HasPrecision(10, 2);

            // Legacy distance field
            builder.Property(x => x.DistanceKm)
                .HasPrecision(10, 2);

            // New multi-stop distance field
            builder.Property(x => x.TotalDistanceKm)
                .HasPrecision(10, 2);

            // Duration fields
            builder.Property(x => x.EstimatedDurationMinutes);

            builder.Property(x => x.EstimatedTotalDurationMinutes);

          

            // Contact fields
            builder.Property(x => x.ContactPersonName)
                .HasMaxLength(100);

            builder.Property(x => x.ContactPhone)
                .HasMaxLength(20);

            builder.Property(x => x.ContactEmail)
                .HasMaxLength(100);

            // Instructions (Legacy)
            builder.Property(x => x.PickupInstructions)
                .HasMaxLength(500);

            builder.Property(x => x.DeliveryInstructions)
                .HasMaxLength(500);

            // Base entity fields
            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.Property(x => x.CreatedUser)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.UpdatedDate)
                .IsRequired();

            builder.Property(x => x.UpdatedUser)
                .IsRequired()
                .HasMaxLength(100);

            // Navigation Properties
            builder.HasMany(x => x.LoadStops)
                .WithOne(ls => ls.Load)
                .HasForeignKey(ls => ls.LoadId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(x => x.CompanyId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.LoadType);
            builder.HasIndex(x => x.IsMultiStop);
            builder.HasIndex(x => x.RoutingStrategy);
            builder.HasIndex(x => x.PickupDateTime);
            builder.HasIndex(x => x.DeliveryDeadline);
            builder.HasIndex(x => x.EarliestPickupTime);
            builder.HasIndex(x => x.LatestDeliveryTime);
            builder.HasIndex(x => x.CreatedDate);
            builder.HasIndex(x => new { x.Status, x.PickupDateTime });
            builder.HasIndex(x => new { x.IsDeleted, x.Status });
            builder.HasIndex(x => new { x.IsMultiStop, x.Status });
            builder.HasIndex(x => new { x.EarliestPickupTime, x.LatestDeliveryTime });
        }
    }
}