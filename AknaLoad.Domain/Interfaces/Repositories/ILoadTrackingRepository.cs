using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface ILoadTrackingRepository : IBaseRepository<LoadTracking>
    {
        Task<List<LoadTracking>> GetTrackingHistoryAsync(long loadId);

        Task<LoadTracking?> GetLatestTrackingAsync(long loadId);

        Task<List<LoadTracking>> GetTrackingByDriverAsync(long driverId, DateTime? fromDate = null, DateTime? toDate = null);

        Task<List<LoadTracking>> GetTrackingByMatchAsync(long matchId);

        Task<List<LoadTracking>> GetActiveTrackingsAsync();

        Task<List<LoadTracking>> GetTrackingsWithExceptionsAsync(bool unresolved = true);

        Task UpdateLocationAsync(long loadId, Location location, decimal? speed = null, int? heading = null, string updatedBy = "");

        Task UpdateStatusAsync(long loadId, TrackingStatus status, string? notes = null, string updatedBy = "");

        Task ReportExceptionAsync(
            long loadId,
            string exceptionType,
            string description,
            string updatedBy);

        Task ResolveExceptionAsync(long trackingId, string updatedBy);

        Task AddDigitalSignatureAsync(
            long loadId,
            string signature,
            string recipientName,
            string? recipientIdNumber = null,
            string updatedBy = "");

        Task AddDocumentationAsync(
            long loadId,
            List<string>? photoUrls = null,
            List<string>? documentUrls = null,
            string? notes = null,
            string updatedBy = "");

        Task<List<LoadTracking>> GetDelayedLoadsAsync(int delayThresholdMinutes = 30);

        Task<List<LoadTracking>> GetOffRouteLoadsAsync(decimal deviationThresholdKm = 5.0m);

        Task<decimal> GetAverageDeliveryPerformanceAsync(
            long? driverId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<List<LoadTracking>> SearchTrackingsAsync(
            TrackingStatus? status = null,
            long? loadId = null,
            long? driverId = null,
            long? matchId = null,
            bool? hasException = null,
            bool? isOnTime = null,
            bool? isOnRoute = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20);
    }
}