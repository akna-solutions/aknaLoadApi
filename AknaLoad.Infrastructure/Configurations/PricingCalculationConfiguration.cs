using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Domain.Configurations
{
    public class PricingCalculationConfiguration : IEntityTypeConfiguration<PricingCalculation>
    {
        public void Configure(EntityTypeBuilder<PricingCalculation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.LoadId)
                .IsRequired();

            builder.Property(x => x.AlgorithmVersion)
                .IsRequired()
                .HasMaxLength(50);

            // Price fields with precision
            builder.Property(x => x.CalculatedPrice)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(x => x.BasePrice)
                .HasPrecision(10, 2);

            builder.Property(x => x.DistanceFactor)
                .HasPrecision(8, 4);

            builder.Property(x => x.WeightFactor)
                .HasPrecision(8, 4);

            builder.Property(x => x.VolumeFactor)
                .HasPrecision(8, 4);

            builder.Property(x => x.UrgencyFactor)
                .HasPrecision(8, 4);

            builder.Property(x => x.DemandFactor)
                .HasPrecision(8, 4);

            builder.Property(x => x.SeasonalFactor)
                .HasPrecision(8, 4);

            builder.Property(x => x.SpecialRequirementsFactor)
                .HasPrecision(8, 4);

            // Cost estimates
            builder.Property(x => x.FuelCostEstimate)
                .HasPrecision(10, 2);

            builder.Property(x => x.TollCostEstimate)
                .HasPrecision(10, 2);

            builder.Property(x => x.DriverCostEstimate)
                .HasPrecision(10, 2);

            builder.Property(x => x.VehicleCostEstimate)
                .HasPrecision(10, 2);

            builder.Property(x => x.ManualAdjustment)
                .HasPrecision(10, 2);

            builder.Property(x => x.ManualAdjustmentReason)
                .HasMaxLength(500);


            builder.Property(x => x.AcceptedAt);

            builder.Property(x => x.FinalAgreedPrice)
                .HasPrecision(10, 2);

            builder.Property(x => x.PriceVariancePercentage)
                .HasPrecision(8, 4);

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
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(x => x.LoadId);
            builder.HasIndex(x => x.AlgorithmVersion);
            builder.HasIndex(x => x.CalculatedAt);
            builder.HasIndex(x => x.WasAccepted);
            builder.HasIndex(x => new { x.LoadId, x.CalculatedAt });
            builder.HasIndex(x => new { x.IsDeleted, x.CalculatedAt });
        }
    }
}