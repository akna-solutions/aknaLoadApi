using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Domain.Configurations
{
    public class LoadTrackingConfiguration : IEntityTypeConfiguration<LoadTracking>
    {
        public void Configure(EntityTypeBuilder<LoadTracking> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.LoadId)
                .IsRequired();

            builder.Property(x => x.DriverId)
                .IsRequired();

            builder.Property(x => x.MatchId)
                .IsRequired();

            // Status and location
            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

        

            builder.Property(x => x.Speed)
                .HasPrecision(6, 2);


            // Timing information
            builder.Property(x => x.EstimatedPickupTime);

            builder.Property(x => x.EstimatedDeliveryTime);

            builder.Property(x => x.ActualPickupTime);

            builder.Property(x => x.ActualDeliveryTime);

            builder.Property(x => x.Timestamp)
                .IsRequired();

            // Progress information
            builder.Property(x => x.DistanceRemaining)
                .HasPrecision(10, 2);



            builder.Property(x => x.ProgressPercentage)
                .HasPrecision(5, 2);

            // Documentation
            builder.Property(x => x.Notes)
                .HasMaxLength(1000);


            builder.Property(x => x.RecipientName)
                .HasMaxLength(100);

            builder.Property(x => x.RecipientIdNumber)
                .HasMaxLength(50);

  
            builder.Property(x => x.ExceptionType)
                .HasMaxLength(50);

            builder.Property(x => x.ExceptionDescription)
                .HasMaxLength(1000);

            builder.Property(x => x.ExceptionReportedAt);

            builder.Property(x => x.ExceptionResolvedAt);

            // Communication
            builder.Property(x => x.DriverMessage)
                .HasMaxLength(500);

            builder.Property(x => x.CustomerMessage)
                .HasMaxLength(500);

            builder.Property(x => x.LastMessageAt);

            // Vehicle information
            builder.Property(x => x.VehiclePlate)
                .HasMaxLength(20);

            builder.Property(x => x.FuelLevel)
                .HasPrecision(5, 2);

    

            builder.Property(x => x.Odometer)
                .HasPrecision(10, 2);




            builder.Property(x => x.RouteDeviation)
                .HasPrecision(10, 2);

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


            // Relationships
            builder.HasOne(x => x.Load)
                .WithMany()
                .HasForeignKey(x => x.LoadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Match)
                .WithMany()
                .HasForeignKey(x => x.MatchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            builder.HasIndex(x => x.LoadId);
            builder.HasIndex(x => x.DriverId);
            builder.HasIndex(x => x.MatchId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => x.HasException);
            builder.HasIndex(x => new { x.LoadId, x.Status });
            builder.HasIndex(x => new { x.DriverId, x.Timestamp });
            builder.HasIndex(x => new { x.Status, x.IsDeleted });
        }
    }
}