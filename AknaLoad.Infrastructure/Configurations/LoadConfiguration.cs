using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Infrastructure.Configurations
{
    public class LoadConfiguration : IEntityTypeConfiguration<Load>
    {
        public void Configure(EntityTypeBuilder<Load> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.OwnerId)
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

            // DateTime fields
            builder.Property(x => x.PickupDateTime)
                .IsRequired();

            builder.Property(x => x.DeliveryDeadline)
                .IsRequired();

            // Decimal fields with precision
            builder.Property(x => x.Weight)
                .HasPrecision(10, 2);

            builder.Property(x => x.Volume)
                .HasPrecision(10, 3);

            builder.Property(x => x.FixedPrice)
                .HasPrecision(10, 2);

            builder.Property(x => x.DistanceKm)
                .HasPrecision(10, 2);

            // Contact fields
            builder.Property(x => x.ContactPersonName)
                .HasMaxLength(100);

            builder.Property(x => x.ContactPhone)
                .HasMaxLength(20);

            builder.Property(x => x.ContactEmail)
                .HasMaxLength(100);

            // Instructions
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

            // Indexes for performance
            builder.HasIndex(x => x.OwnerId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.LoadType);
            builder.HasIndex(x => x.PickupDateTime);
            builder.HasIndex(x => x.DeliveryDeadline);
            builder.HasIndex(x => x.CreatedDate);
            builder.HasIndex(x => new { x.Status, x.PickupDateTime });
            builder.HasIndex(x => new { x.IsDeleted, x.Status });
        }
    }
}