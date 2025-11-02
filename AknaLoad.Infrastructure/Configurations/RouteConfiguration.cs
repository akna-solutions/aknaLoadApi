using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Domain.Configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.RouteCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.RouteCode)
                .IsUnique();


            // Route details with precision
            builder.Property(x => x.TotalDistance)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(x => x.EstimatedDuration)
                .IsRequired();

            builder.Property(x => x.TollCost)
                .HasPrecision(10, 2);

            builder.Property(x => x.FuelCost)
                .HasPrecision(10, 2);

            builder.Property(x => x.RouteType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.RoadConditions)
                .HasMaxLength(500);

            builder.Property(x => x.WeatherConditions)
                .HasMaxLength(500);

           
            // Vehicle restrictions
            builder.Property(x => x.MaxVehicleHeight)
                .HasPrecision(5, 2);

            builder.Property(x => x.MaxVehicleWidth)
                .HasPrecision(5, 2);

            builder.Property(x => x.MaxVehicleWeight)
                .HasPrecision(8, 2);


            // Time windows
            builder.Property(x => x.EarliestDepartureTime);

            builder.Property(x => x.LatestArrivalTime);

        
            builder.Property(x => x.LastUsedAt);

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
            // Indexes for performance
            builder.HasIndex(x => x.RouteType);
            builder.HasIndex(x => x.TotalDistance);
            builder.HasIndex(x => x.EstimatedDuration);
            builder.HasIndex(x => x.UsageCount);
            builder.HasIndex(x => x.LastUsedAt);
            builder.HasIndex(x => x.IsOptimized);
            builder.HasIndex(x => new { x.IsDeleted, x.RouteType });
            builder.HasIndex(x => new { x.TotalDistance, x.EstimatedDuration });
        }
    }
}