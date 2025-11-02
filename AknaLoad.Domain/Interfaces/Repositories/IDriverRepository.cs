using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface IDriverRepository : IBaseRepository<Driver>
    {
        Task<List<Driver>> GetAvailableDriversAsync(
            Location? loadLocation = null,
            int maxDistanceKm = 500,
            DateTime? availableFrom = null,
            DateTime? availableUntil = null);

        Task<List<Driver>> GetDriversByCompanyAsync(long companyId, DriverAvailabilityStatus? status = null);

        Task<Driver?> GetDriverByCodeAsync(string driverCode);

        Task<Driver?> GetDriverByUserIdAsync(long userId);

        Task<List<Driver>> GetDriversByLicenseCategoryAsync(string licenseCategory);

        Task<List<Driver>> GetDriversWithSpecialSkillsAsync(List<SpecialRequirement> requirements);

        Task UpdateDriverLocationAsync(long driverId, Location location, string updatedBy);

        Task UpdateDriverStatusAsync(long driverId, DriverAvailabilityStatus status, string updatedBy);

        Task UpdateDriverRatingAsync(long driverId, decimal newRating, string updatedBy);

        Task<List<Driver>> GetTopRatedDriversAsync(int count = 10, decimal minRating = 4.0m);

        Task<List<Driver>> GetDriversInAreaAsync(GeographicArea area);

        Task<List<Driver>> SearchDriversAsync(
            string? keyword = null,
            DriverAvailabilityStatus? status = null,
            string? licenseCategory = null,
            decimal? minRating = null,
            string? city = null,
            bool? hasADRLicense = null,
            bool? hasSRCLicense = null,
            int pageNumber = 1,
            int pageSize = 20);

        Task<Driver?> GetDriverWithVehiclesAsync(long driverId);

        Task IncrementCompletedLoadsAsync(long driverId, string updatedBy);
    }
}