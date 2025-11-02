using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Infrastructure.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.CompanyId)
                .IsRequired();

            builder.Property(x => x.DriverCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.DriverCode)
                .IsUnique();

            // License Information
            builder.Property(x => x.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.LicenseCategory)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.ExperienceYears)
                .IsRequired();

            // Status and Availability
            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            // Indexes for performance
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.CompanyId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.LicenseNumber);
            builder.HasIndex(x => x.CurrentVehicleId);
            builder.HasIndex(x => x.LastActiveAt);
            builder.HasIndex(x => new { x.Status, x.IsDeleted });
            builder.HasIndex(x => new { x.CompanyId, x.Status });
        }
    }
}