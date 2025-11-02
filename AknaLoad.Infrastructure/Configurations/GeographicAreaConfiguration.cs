using AknaLoad.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AknaLoad.Domain.Configurations
{
    public class GeographicAreaConfiguration : IEntityTypeConfiguration<GeographicArea>
    {
        public void Configure(EntityTypeBuilder<GeographicArea> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.AreaType)
                .IsRequired()
                .HasConversion<int>();


            builder.Property(x => x.Province)
                .HasMaxLength(100);

            builder.Property(x => x.City)
                .HasMaxLength(100);

            builder.Property(x => x.District)
                .HasMaxLength(100);

            builder.Property(x => x.PostalCodes)
                .HasMaxLength(1000);

       

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
            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.AreaType);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.Country);
            builder.HasIndex(x => x.Province);
            builder.HasIndex(x => x.City);
            builder.HasIndex(x => x.Priority);
            builder.HasIndex(x => new { x.AreaType, x.IsActive });
            builder.HasIndex(x => new { x.Country, x.Province, x.City });
            builder.HasIndex(x => new { x.IsDeleted, x.IsActive });
        }
    }
}