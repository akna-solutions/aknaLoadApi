using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using AknaLoad.Infrastructure.Persistence;

namespace AknaLoad.Infrastructure.Repositories
{
    public class RouteRepository : BaseRepository<Route>, IRouteRepository
    {
        private readonly AknaLoadDbContext _context;

        public RouteRepository(AknaLoadDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Route?> GetRouteByCodeAsync(string routeCode)
        {
            return await _context.Routes
                .Include(r => r.Load)
                .FirstOrDefaultAsync(r => r.RouteCode == routeCode);
        }

        public async Task<bool> IsRouteCodeUniqueAsync(string routeCode)
        {
            return !await _context.Routes.AnyAsync(r => r.RouteCode == routeCode);
        }

        public async Task<List<Route>> FindSimilarRoutesAsync(
            Location startLocation,
            Location endLocation,
            double toleranceKm = 5.0)
        {
            // This is a simplified implementation
            // In production, you would use more sophisticated geospatial queries
            var routes = await _context.Routes
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            return routes
                .Where(r =>
                {
                    var start = r.StartLocation;
                    var end = r.EndLocation;

                    if (start == null || end == null) return false;

                    var startDistance = startLocation.DistanceTo(start);
                    var endDistance = endLocation.DistanceTo(end);

                    return startDistance <= toleranceKm && endDistance <= toleranceKm;
                })
                .ToList();
        }

        public async Task<Route?> GetOptimalRouteAsync(
            Location startLocation,
            Location endLocation,
            RouteType routeType = RouteType.Optimal)
        {
            var similarRoutes = await FindSimilarRoutesAsync(startLocation, endLocation);

            return similarRoutes
                .Where(r => r.RouteType == routeType)
                .OrderByDescending(r => r.UsageCount)
                .ThenBy(r => r.TotalDistance)
                .FirstOrDefault();
        }

        public async Task<List<Route>> GetRoutesByTypeAsync(RouteType routeType)
        {
            return await _context.Routes
                .Where(r => r.RouteType == routeType && !r.IsDeleted)
                .OrderBy(r => r.TotalDistance)
                .ToListAsync();
        }

        public async Task<List<Route>> GetMostUsedRoutesAsync(int count = 20)
        {
            return await _context.Routes
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.UsageCount)
                .ThenBy(r => r.TotalDistance)
                .Take(count)
                .ToListAsync();
        }

        public async Task IncrementUsageCountAsync(long routeId, string updatedBy)
        {
            var route = await _context.Routes.FindAsync(routeId);
            if (route != null)
            {
                route.UsageCount++;
                route.LastUsedAt = DateTime.UtcNow;
                route.UpdatedUser = updatedBy;
                route.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateRoutePerformanceAsync(
            long routeId,
            decimal actualDistance,
            int actualDurationMinutes,
            string updatedBy)
        {
            var route = await _context.Routes.FindAsync(routeId);
            if (route != null)
            {
                // Update running averages
                var totalUsages = route.UsageCount + 1;
                route.AverageActualDistance = ((route.AverageActualDistance * route.UsageCount) + actualDistance) / totalUsages;
                route.AverageActualDuration = ((route.AverageActualDuration * route.UsageCount) + actualDurationMinutes) / totalUsages;

                route.UsageCount = (int)totalUsages;
                route.LastUsedAt = DateTime.UtcNow;
                route.UpdatedUser = updatedBy;
                route.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Route>> GetRoutesWithinDistanceAsync(
            decimal minDistance,
            decimal maxDistance)
        {
            return await _context.Routes
                .Where(r => r.TotalDistance >= minDistance && r.TotalDistance <= maxDistance && !r.IsDeleted)
                .OrderBy(r => r.TotalDistance)
                .ToListAsync();
        }

        public async Task<List<Route>> GetRoutesWithRestrictionsAsync(
            decimal? maxHeight = null,
            decimal? maxWidth = null,
            decimal? maxWeight = null,
            bool? truckRestrictions = null,
            bool? hazmatRestrictions = null)
        {
            var query = _context.Routes.Where(r => !r.IsDeleted);

            if (maxHeight.HasValue)
                query = query.Where(r => !r.MaxVehicleHeight.HasValue || r.MaxVehicleHeight >= maxHeight.Value);

            if (maxWidth.HasValue)
                query = query.Where(r => !r.MaxVehicleWidth.HasValue || r.MaxVehicleWidth >= maxWidth.Value);

            if (maxWeight.HasValue)
                query = query.Where(r => !r.MaxVehicleWeight.HasValue || r.MaxVehicleWeight >= maxWeight.Value);

            if (truckRestrictions.HasValue)
                query = query.Where(r => r.HasTruckRestrictions == truckRestrictions.Value);

            if (hazmatRestrictions.HasValue)
                query = query.Where(r => r.HasHazmatRestrictions == hazmatRestrictions.Value);

            return await query
                .OrderBy(r => r.TotalDistance)
                .ToListAsync();
        }

        public async Task<decimal> GetAverageRouteEfficiencyAsync(long routeId)
        {
            var route = await _context.Routes.FindAsync(routeId);
            if (route == null || route.UsageCount == 0 || route.AverageActualDistance == 0)
                return 0;

            // Efficiency = Estimated / Actual (values > 1 mean route was faster than estimated)
            return route.TotalDistance / route.AverageActualDistance;
        }

        public async Task<List<Route>> SearchRoutesAsync(
            string? keyword = null,
            RouteType? routeType = null,
            decimal? minDistance = null,
            decimal? maxDistance = null,
            int? minDuration = null,
            int? maxDuration = null,
            bool? isOptimized = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = _context.Routes.Where(r => !r.IsDeleted);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(r => r.RouteCode.Contains(keyword));

            if (routeType.HasValue)
                query = query.Where(r => r.RouteType == routeType.Value);

            if (minDistance.HasValue)
                query = query.Where(r => r.TotalDistance >= minDistance.Value);

            if (maxDistance.HasValue)
                query = query.Where(r => r.TotalDistance <= maxDistance.Value);

            if (minDuration.HasValue)
                query = query.Where(r => r.EstimatedDuration >= minDuration.Value);

            if (maxDuration.HasValue)
                query = query.Where(r => r.EstimatedDuration <= maxDuration.Value);

            if (isOptimized.HasValue)
                query = query.Where(r => r.IsOptimized == isOptimized.Value);

            return await query
                .OrderByDescending(r => r.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Route>> GetRoutesByTimeWindowAsync(
            DateTime? earliestDeparture = null,
            DateTime? latestArrival = null)
        {
            var query = _context.Routes.Where(r => !r.IsDeleted);

            if (earliestDeparture.HasValue)
                query = query.Where(r => !r.EarliestDepartureTime.HasValue || r.EarliestDepartureTime >= earliestDeparture.Value);

            if (latestArrival.HasValue)
                query = query.Where(r => !r.LatestArrivalTime.HasValue || r.LatestArrivalTime <= latestArrival.Value);

            return await query
                .OrderBy(r => r.EarliestDepartureTime)
                .ToListAsync();
        }
    }
}
