using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using AknaLoad.Infrastructure.Persistence;

namespace AknaLoad.Infrastructure.Repositories
{
    public class DriverRepository : BaseRepository<Driver>, IDriverRepository
    {
        public DriverRepository(AknaLoadDbContext context) : base(context)
        {
        }

        public async Task<List<Driver>> GetAvailableDriversAsync(
            Location? loadLocation = null,
            int maxDistanceKm = 500,
            DateTime? availableFrom = null,
            DateTime? availableUntil = null)
        {
            var query = _dbSet.Where(d => d.Status == DriverAvailabilityStatus.Available);

            if (availableFrom.HasValue)
                query = query.Where(d => !d.AvailableFrom.HasValue || d.AvailableFrom <= availableFrom.Value);

            if (availableUntil.HasValue)
                query = query.Where(d => !d.AvailableUntil.HasValue || d.AvailableUntil >= availableUntil.Value);

            query = query.Where(d => d.MaxDistanceKm >= maxDistanceKm);

            var drivers = await query.AsNoTracking().ToListAsync();

            // Filter by distance if load location is provided
            if (loadLocation != null)
            {
                drivers = drivers.Where(d =>
                {
                    if (d.CurrentLocation != null)
                    {
                        var distance = loadLocation.DistanceTo(d.CurrentLocation);
                        return distance <= d.MaxDistanceKm && distance <= maxDistanceKm;
                    }
                    return false;
                }).ToList();
            }

            return drivers.OrderByDescending(d => d.AverageRating).ToList();
        }

        public async Task<List<Driver>> GetDriversByCompanyAsync(long companyId, DriverAvailabilityStatus? status = null)
        {
            var query = _dbSet.Where(d => d.CompanyId == companyId);

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Driver?> GetDriverByCodeAsync(string driverCode)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.DriverCode == driverCode);
        }

        public async Task<Driver?> GetDriverByUserIdAsync(long userId)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<List<Driver>> GetDriversByLicenseCategoryAsync(string licenseCategory)
        {
            return await _dbSet
                .Where(d => d.LicenseCategory == licenseCategory)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Driver>> GetDriversWithSpecialSkillsAsync(List<SpecialRequirement> requirements)
        {
            var drivers = await _dbSet.AsNoTracking().ToListAsync();

            return drivers.Where(d =>
            {
                foreach (var requirement in requirements)
                {
                    switch (requirement)
                    {
                        case SpecialRequirement.Hazardous:
                            if (!d.HasADRLicense) return false;
                            break;
                        case SpecialRequirement.ColdChain:
                        case SpecialRequirement.TemperatureControlled:
                            // Check if driver has refrigerated vehicle experience
                            break;
                            // Add more special requirement checks
                    }
                }
                return true;
            }).ToList();
        }

        public async Task UpdateDriverLocationAsync(long driverId, Location location, string updatedBy)
        {
            var driver = await GetByIdAsync(driverId);
            if (driver != null)
            {
                driver.CurrentLocation = location;
                driver.LastLocationUpdateAt = DateTime.UtcNow;
                driver.UpdatedUser = updatedBy;
                Update(driver);
            }
        }

        public async Task UpdateDriverStatusAsync(long driverId, DriverAvailabilityStatus status, string updatedBy)
        {
            var driver = await GetByIdAsync(driverId);
            if (driver != null)
            {
                driver.Status = status;
                driver.LastActiveAt = DateTime.UtcNow;
                driver.UpdatedUser = updatedBy;
                Update(driver);
            }
        }

        public async Task UpdateDriverRatingAsync(long driverId, decimal newRating, string updatedBy)
        {
            var driver = await GetByIdAsync(driverId);
            if (driver != null)
            {
                var totalRatings = driver.TotalRatings;
                var currentTotalScore = driver.AverageRating * totalRatings;

                driver.TotalRatings = totalRatings + 1;
                driver.AverageRating = (currentTotalScore + newRating) / driver.TotalRatings;
                driver.UpdatedUser = updatedBy;

                Update(driver);
            }
        }

        public async Task<List<Driver>> GetTopRatedDriversAsync(int count = 10, decimal minRating = 4.0m)
        {
            return await _dbSet
                .Where(d => d.AverageRating >= minRating && d.TotalRatings > 0)
                .OrderByDescending(d => d.AverageRating)
                .ThenByDescending(d => d.CompletedLoads)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Driver>> GetDriversInAreaAsync(GeographicArea area)
        {
            var drivers = await _dbSet.AsNoTracking().ToListAsync();

            return drivers.Where(d =>
            {
                if (d.CurrentLocation != null)
                {
                    return area.ContainsPoint(d.CurrentLocation);
                }
                return false;
            }).ToList();
        }

        public async Task<List<Driver>> SearchDriversAsync(
            string? keyword = null,
            DriverAvailabilityStatus? status = null,
            string? licenseCategory = null,
            decimal? minRating = null,
            string? city = null,
            bool? hasADRLicense = null,
            bool? hasSRCLicense = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d => d.DriverCode.Contains(keyword) ||
                                        d.LicenseNumber.Contains(keyword));
            }

            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            if (!string.IsNullOrEmpty(licenseCategory))
                query = query.Where(d => d.LicenseCategory == licenseCategory);

            if (minRating.HasValue)
                query = query.Where(d => d.AverageRating >= minRating.Value);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(d => d.CurrentLocationJson.Contains($"\"City\":\"{city}\""));

            if (hasADRLicense.HasValue)
                query = query.Where(d => d.HasADRLicense == hasADRLicense.Value);

            if (hasSRCLicense.HasValue)
                query = query.Where(d => d.HasSRCLicense == hasSRCLicense.Value);

            return await query
                .OrderByDescending(d => d.AverageRating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Driver?> GetDriverWithVehiclesAsync(long driverId)
        {
            // This would need to join with Vehicle entity from Identity service
            // For now, just return the driver
            return await GetByIdAsync(driverId);
        }

        public async Task IncrementCompletedLoadsAsync(long driverId, string updatedBy)
        {
            var driver = await GetByIdAsync(driverId);
            if (driver != null)
            {
                driver.CompletedLoads++;
                driver.UpdatedUser = updatedBy;
                Update(driver);
            }
        }
    }
}