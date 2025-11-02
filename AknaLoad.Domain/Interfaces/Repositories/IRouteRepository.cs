using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface IRouteRepository : IBaseRepository<Route>
    {
        Task<Route?> GetRouteByCodeAsync(string routeCode);

        Task<List<Route>> FindSimilarRoutesAsync(
            Location startLocation,
            Location endLocation,
            double toleranceKm = 5.0);

        Task<Route?> GetOptimalRouteAsync(
            Location startLocation,
            Location endLocation,
            RouteType routeType = RouteType.Optimal);

        Task<List<Route>> GetRoutesByTypeAsync(RouteType routeType);

        Task<List<Route>> GetMostUsedRoutesAsync(int count = 20);

        Task IncrementUsageCountAsync(long routeId, string updatedBy);

        Task UpdateRoutePerformanceAsync(
            long routeId,
            decimal actualDistance,
            int actualDurationMinutes,
            string updatedBy);

        Task<List<Route>> GetRoutesWithinDistanceAsync(
            decimal minDistance,
            decimal maxDistance);

        Task<List<Route>> GetRoutesWithRestrictionsAsync(
            decimal? maxHeight = null,
            decimal? maxWidth = null,
            decimal? maxWeight = null,
            bool? truckRestrictions = null,
            bool? hazmatRestrictions = null);

        Task<decimal> GetAverageRouteEfficiencyAsync(long routeId);

        Task<List<Route>> SearchRoutesAsync(
            string? keyword = null,
            RouteType? routeType = null,
            decimal? minDistance = null,
            decimal? maxDistance = null,
            int? minDuration = null,
            int? maxDuration = null,
            bool? isOptimized = null,
            int pageNumber = 1,
            int pageSize = 20);

        Task<List<Route>> GetRoutesByTimeWindowAsync(
            DateTime? earliestDeparture = null,
            DateTime? latestArrival = null);
    }
}