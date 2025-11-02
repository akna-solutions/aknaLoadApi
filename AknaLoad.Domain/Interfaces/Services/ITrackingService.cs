using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;

namespace AknaLoad.Domain.Interfaces.Services
{
    public interface ITrackingService
    {
        Task<LoadTracking> StartTrackingAsync(long loadId, long driverId, long matchId, string startedBy);
        Task<bool> UpdateLocationAsync(long loadId, Location location, decimal? speed = null, int? heading = null, string updatedBy = "");
        Task<bool> UpdateStatusAsync(long loadId, TrackingStatus status, string? notes = null, string updatedBy = "");
        Task<LoadTracking?> GetLatestTrackingAsync(long loadId);
        Task<List<LoadTracking>> GetTrackingHistoryAsync(long loadId);
        Task<List<LoadTracking>> GetActiveTrackingsAsync();
        Task<bool> ReportExceptionAsync(long loadId, string exceptionType, string description, string reportedBy);
        Task<bool> ResolveExceptionAsync(long trackingId, string resolvedBy);
        Task<bool> CompleteDeliveryAsync(long loadId, string signature, string recipientName, string? recipientIdNumber = null, string completedBy = "");
        Task<bool> AddPhotosAsync(long loadId, List<string> photoUrls, string addedBy);
        Task<bool> AddDocumentsAsync(long loadId, List<string> documentUrls, string addedBy);
        Task<List<LoadTracking>> GetDelayedLoadsAsync(int delayThresholdMinutes = 30);
        Task<List<LoadTracking>> GetOffRouteLoadsAsync(decimal deviationThresholdKm = 5.0m);
        Task<List<LoadTracking>> GetTrackingsWithExceptionsAsync(bool unresolvedOnly = true);
        Task<decimal> CalculateProgressPercentageAsync(long loadId);
        Task<DateTime?> GetEstimatedArrivalTimeAsync(long loadId);
        Task<bool> SendLocationNotificationAsync(long loadId, string recipientType);
        Task<decimal> GetDeliveryPerformanceAsync(long? driverId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> ValidateDeliveryLocationAsync(long loadId, Location currentLocation, decimal toleranceMeters = 100);
        Task<List<LoadTracking>> SearchTrackingsAsync(
            TrackingStatus? status = null,
            long? loadId = null,
            long? driverId = null,
            bool? hasException = null,
            bool? isOnTime = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20);
    }
}