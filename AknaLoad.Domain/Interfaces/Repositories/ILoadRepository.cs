using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;

namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface ILoadRepository : IBaseRepository<Load>
    {
        Task<List<Load>> GetAvailableLoadsAsync(
            Location? driverLocation = null,
            int maxDistanceKm = 500,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<List<Load>> GetLoadsByOwnerAsync(long ownerId, LoadStatus? status = null);

        Task<List<Load>> GetLoadsByLocationAsync(
            string city,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<Load?> GetLoadByCodeAsync(string loadCode);

        Task<List<Load>> GetExpiringLoadsAsync(DateTime beforeDate);

        Task<decimal?> GetAveragePriceByRouteAsync(
            Location pickupLocation,
            Location deliveryLocation,
            LoadType loadType);

        Task UpdateLoadStatusAsync(long loadId, LoadStatus status, string updatedBy);

        Task<bool> AssignLoadToDriverAsync(long loadId, long driverId, long vehicleId, string updatedBy);

        Task<List<Load>> SearchLoadsAsync(
            string? keyword = null,
            LoadType? loadType = null,
            LoadStatus? status = null,
            decimal? minWeight = null,
            decimal? maxWeight = null,
            string? pickupCity = null,
            string? deliveryCity = null,
            DateTime? pickupFromDate = null,
            DateTime? pickupToDate = null,
            int pageNumber = 1,
            int pageSize = 20);
    }
}