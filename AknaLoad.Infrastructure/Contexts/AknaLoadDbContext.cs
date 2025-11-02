using Microsoft.EntityFrameworkCore;
using AknaLoad.Domain.Configurations;
using AknaLoad.Domain.Entities;
using AknaLoad.Infrastructure.Configurations;
using AknaLoad.Domain.Entities.BaseEnities;

namespace AknaLoad.Infrastructure.Persistence
{
    public class AknaLoadDbContext : DbContext
    {
        public AknaLoadDbContext(DbContextOptions<AknaLoadDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Load> Loads { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<PricingCalculation> PricingCalculations { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<LoadTracking> LoadTrackings { get; set; }
        public DbSet<GeographicArea> GeographicAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations
            modelBuilder.ApplyConfiguration(new LoadConfiguration());
            modelBuilder.ApplyConfiguration(new DriverConfiguration());
            modelBuilder.ApplyConfiguration(new MatchConfiguration());
            modelBuilder.ApplyConfiguration(new PricingCalculationConfiguration());
            modelBuilder.ApplyConfiguration(new RouteConfiguration());
            modelBuilder.ApplyConfiguration(new LoadTrackingConfiguration());
            modelBuilder.ApplyConfiguration(new GeographicAreaConfiguration());

            // Global query filters for soft delete
            modelBuilder.Entity<Load>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Driver>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Match>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PricingCalculation>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Route>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<LoadTracking>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<GeographicArea>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is BaseEntity baseEntity)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        baseEntity.CreatedDate = DateTime.UtcNow;
                        baseEntity.UpdatedDate = DateTime.UtcNow;

                        if (string.IsNullOrEmpty(baseEntity.CreatedUser))
                            baseEntity.CreatedUser = "System";
                        if (string.IsNullOrEmpty(baseEntity.UpdatedUser))
                            baseEntity.UpdatedUser = "System";
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        baseEntity.UpdatedDate = DateTime.UtcNow;

                        if (string.IsNullOrEmpty(baseEntity.UpdatedUser))
                            baseEntity.UpdatedUser = "System";
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}