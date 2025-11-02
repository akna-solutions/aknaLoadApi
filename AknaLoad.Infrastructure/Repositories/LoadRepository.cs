using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using AknaLoad.Infrastructure.Persistence;

namespace AknaLoad.Infrastructure.Repositories
{
    public class LoadRepository : BaseRepository<Load>, ILoadRepository
    {
        public LoadRepository(AknaLoadDbContext context) : base(context)
        {
        }

        public async Task<List<Load>> GetAvailableLoadsAsync(
            Location? driverLocation = null,
            int maxDistanceKm = 500,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _dbSet.Where(l => l.Status == LoadStatus.Published);

            if (fromDate.HasValue)
                query = query.Where(l => l.PickupDateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.PickupDateTime <= toDate.Value);

            var loads = await query.AsNoTracking().ToListAsync();

            // Filter by distance if driver location is provided
            if (driverLocation != null)
            {
                loads = loads.Where(l =>
                {
                    if (l.PickupLocation != null)
                    {
                        var distance = driverLocation.DistanceTo(l.PickupLocation);
                        return distance <= maxDistanceKm;
                    }
                    return false;
                }).ToList();
            }

            return loads;
        }

        public async Task<List<Load>> GetLoadsByOwnerAsync(long ownerId, LoadStatus? status = null)
        {
            var query = _dbSet.Where(l => l.OwnerId == ownerId);

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<List<Load>> GetLoadsByLocationAsync(
            string city,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _dbSet.Where(l => l.PickupLocationJson.Contains($"\"City\":\"{city}\"") ||
                                         l.DeliveryLocationJson.Contains($"\"City\":\"{city}\""));

            if (fromDate.HasValue)
                query = query.Where(l => l.PickupDateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.PickupDateTime <= toDate.Value);

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<Load?> GetLoadByCodeAsync(string loadCode)
        {
            return await _dbSet.FirstOrDefaultAsync(l => l.LoadCode == loadCode);
        }

        public async Task<List<Load>> GetExpiringLoadsAsync(DateTime beforeDate)
        {
            return await _dbSet
                .Where(l => l.Status == LoadStatus.Published && l.DeliveryDeadline <= beforeDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<decimal?> GetAveragePriceByRouteAsync(
            Location pickupLocation,
            Location deliveryLocation,
            LoadType loadType)
        {
            // This is a simplified implementation
            // In real scenario, you'd need more sophisticated location matching
            var loads = await _dbSet
                .Where(l => l.LoadType == loadType && l.FixedPrice.HasValue)
                .AsNoTracking()
                .ToListAsync();

            var relevantLoads = loads.Where(l =>
            {
                if (l.PickupLocation != null && l.DeliveryLocation != null)
                {
                    var pickupDistance = pickupLocation.DistanceTo(l.PickupLocation);
                    var deliveryDistance = deliveryLocation.DistanceTo(l.DeliveryLocation);
                    return pickupDistance <= 50 && deliveryDistance <= 50; // 50km tolerance
                }
                return false;
            }).ToList();

            if (!relevantLoads.Any())
                return null;

            return relevantLoads.Average(l => l.FixedPrice!.Value);
        }

        public async Task UpdateLoadStatusAsync(long loadId, LoadStatus status, string updatedBy)
        {
            var load = await GetByIdAsync(loadId);
            if (load != null)
            {
                load.Status = status;
                load.UpdatedUser = updatedBy;

                if (status == LoadStatus.Published)
                    load.PublishedAt = DateTime.UtcNow;
                else if (status == LoadStatus.Completed)
                    load.CompletedAt = DateTime.UtcNow;
                else if (status == LoadStatus.Matched)
                    load.MatchedAt = DateTime.UtcNow;

                Update(load);
            }
        }

        public async Task<bool> AssignLoadToDriverAsync(long loadId, long driverId, long vehicleId, string updatedBy)
        {
            var load = await GetByIdAsync(loadId);
            if (load == null || load.Status != LoadStatus.Published)
                return false;

            load.MatchedDriverId = driverId;
            load.MatchedVehicleId = vehicleId;
            load.Status = LoadStatus.Matched;
            load.MatchedAt = DateTime.UtcNow;
            load.UpdatedUser = updatedBy;

            Update(load);
            return true;
        }

        public async Task<List<Load>> SearchLoadsAsync(
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
            int pageSize = 20)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(l => l.Title.Contains(keyword) ||
                                        (l.Description != null && l.Description.Contains(keyword)));
            }

            if (loadType.HasValue)
                query = query.Where(l => l.LoadType == loadType.Value);

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);

            if (minWeight.HasValue)
                query = query.Where(l => l.Weight >= minWeight.Value);

            if (maxWeight.HasValue)
                query = query.Where(l => l.Weight <= maxWeight.Value);

            if (!string.IsNullOrEmpty(pickupCity))
                query = query.Where(l => l.PickupLocationJson.Contains($"\"City\":\"{pickupCity}\""));

            if (!string.IsNullOrEmpty(deliveryCity))
                query = query.Where(l => l.DeliveryLocationJson.Contains($"\"City\":\"{deliveryCity}\""));

            if (pickupFromDate.HasValue)
                query = query.Where(l => l.PickupDateTime >= pickupFromDate.Value);

            if (pickupToDate.HasValue)
                query = query.Where(l => l.PickupDateTime <= pickupToDate.Value);

            return await query
                .OrderByDescending(l => l.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}