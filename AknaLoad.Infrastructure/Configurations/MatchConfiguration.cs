using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Domain.Configurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.LoadId)
                .IsRequired();

            builder.Property(x => x.DriverId)
                .IsRequired();

            builder.Property(x => x.VehicleId)
                .IsRequired();

            builder.Property(x => x.MatchCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.MatchCode)
                .IsUnique();

            // Matching algorithm results
            builder.Property(x => x.MatchScore)
                .IsRequired()
                .HasPrecision(5, 2);
            // Status and timing
            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.ProposedAt)
                .IsRequired();

            builder.Property(x => x.NotifiedAt);

            builder.Property(x => x.RespondedAt);

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            // Route information
            builder.Property(x => x.EstimatedPickupTime);

            builder.Property(x => x.EstimatedDeliveryTime);

            builder.Property(x => x.RouteDistance)
                .HasPrecision(10, 2);

            // Actual performance
            builder.Property(x => x.ActualPickupTime);

            builder.Property(x => x.ActualDeliveryTime);

            builder.Property(x => x.ActualDistance)
                .HasPrecision(10, 2);


            // Financial details
            builder.Property(x => x.AgreedPrice)
                .HasPrecision(10, 2);

            builder.Property(x => x.DriverCommission)
                .HasPrecision(10, 2);

            builder.Property(x => x.PlatformFee)
                .HasPrecision(10, 2);

            // Rating and feedback
            builder.Property(x => x.LoadOwnerRating)
                .HasPrecision(2, 1);

            builder.Property(x => x.LoadOwnerFeedback)
                .HasMaxLength(1000);

            builder.Property(x => x.DriverRating)
                .HasPrecision(2, 1);

            builder.Property(x => x.DriverFeedback)
                .HasMaxLength(1000);

            builder.Property(x => x.EmergencyDescription)
                .HasMaxLength(1000);

            builder.Property(x => x.EmergencyReportedAt);

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
                .WithMany(l => l.Matches)
                .HasForeignKey(x => x.LoadId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Driver)
                .WithMany()
                .HasForeignKey(x => x.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            builder.HasIndex(x => x.LoadId);
            builder.HasIndex(x => x.DriverId);
            builder.HasIndex(x => x.VehicleId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.ProposedAt);
            builder.HasIndex(x => x.ExpiresAt);
            builder.HasIndex(x => new { x.LoadId, x.Status });
            builder.HasIndex(x => new { x.DriverId, x.Status });
            builder.HasIndex(x => new { x.Status, x.IsDeleted });
        }
    }
}