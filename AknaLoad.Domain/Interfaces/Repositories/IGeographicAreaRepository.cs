using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface IGeographicAreaRepository : IBaseRepository<GeographicArea>
    {
        Task<List<GeographicArea>> GetAreasContainingPointAsync(Location point);

        Task<List<GeographicArea>> GetAreasByTypeAsync(GeographicAreaType areaType, bool activeOnly = true);

        Task<List<GeographicArea>> GetAreasByCountryAsync(string country, bool activeOnly = true);

        Task<List<GeographicArea>> GetAreasByProvinceAsync(string country, string province, bool activeOnly = true);

        Task<List<GeographicArea>> GetAreasByCityAsync(string country, string city, bool activeOnly = true);

        Task<GeographicArea?> GetAreaByNameAsync(string name);

        Task<List<GeographicArea>> GetAreasWithinRadiusAsync(
            Location centerPoint,
            decimal radiusKm,
            GeographicAreaType? areaType = null);

        Task UpdateAreaUsageAsync(long areaId, bool incrementLoad = false, bool incrementDriver = false, string updatedBy = "");

        Task<List<GeographicArea>> GetMostUsedAreasAsync(int count = 20, GeographicAreaType? areaType = null);

        Task<List<GeographicArea>> GetAreasByPostalCodeAsync(string postalCode);

        Task ActivateAreaAsync(long areaId, string updatedBy);

        Task DeactivateAreaAsync(long areaId, string updatedBy);

        Task<List<GeographicArea>> SearchAreasAsync(
            string? keyword = null,
            GeographicAreaType? areaType = null,
            string? country = null,
            string? province = null,
            string? city = null,
            bool? isActive = null,
            decimal? minRadius = null,
            decimal? maxRadius = null,
            int pageNumber = 1,
            int pageSize = 20);

        Task<bool> IsLocationWithinAnyAreaAsync(Location location, GeographicAreaType? areaType = null);

        Task<decimal> GetAreaCoverageAsync(Location centerPoint, decimal radiusKm);
    }
}