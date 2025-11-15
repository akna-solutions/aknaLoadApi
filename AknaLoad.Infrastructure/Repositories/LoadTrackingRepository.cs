using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AknaLoad.Infrastructure.Repositories
{
    public class LoadTrackingRepository : BaseRepository<LoadTracking>, ILoadTrackingRepository
    {
        private readonly AknaLoadDbContext _context;

        public LoadTrackingRepository(AknaLoadDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Override GetByIdAsync to include navigation properties (Load, Driver, Match)
        /// </summary>
        public new async Task<LoadTracking?> GetByIdAsync(long id, bool trackChanges = true)
        {
            var query = trackChanges ? _context.LoadTrackings.AsTracking() : _context.LoadTrackings.AsNoTracking();

            return await query
                .Include(lt => lt.Load)
                    .ThenInclude(l => l.LoadStops.OrderBy(s => s.StopOrder))
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .FirstOrDefaultAsync(lt => lt.Id == id);
        }

        public async Task<List<LoadTracking>> GetTrackingHistoryAsync(long loadId)
        {
            return await _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => lt.LoadId == loadId)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.Timestamp)
                .ToListAsync();
        }

        public async Task<LoadTracking?> GetLatestTrackingAsync(long loadId)
        {
            return await _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => lt.LoadId == loadId)
                .Include(lt => lt.Load)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LoadTracking>> GetTrackingByDriverAsync(long driverId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => lt.DriverId == driverId);

            if (fromDate.HasValue)
                query = query.Where(lt => lt.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(lt => lt.Timestamp <= toDate.Value);

            return await query
                .Include(lt => lt.Load)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.Timestamp)
                .ToListAsync();
        }

        public async Task<List<LoadTracking>> GetTrackingByMatchAsync(long matchId)
        {
            return await _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => lt.MatchId == matchId)
                .Include(lt => lt.Load)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderBy(lt => lt.Timestamp)
                .ToListAsync();
        }

        public async Task<List<LoadTracking>> GetActiveTrackingsAsync()
        {
            var activeStatuses = new[]
            {
                TrackingStatus.InTransit,
                TrackingStatus.PickedUp,
                TrackingStatus.WaitingForPickup,
                TrackingStatus.ArrivedAtPickup,
                TrackingStatus.ArrivedAtDelivery
            };

            return await _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => activeStatuses.Contains(lt.Status))
                .Include(lt => lt.Load)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.Timestamp)
                .ToListAsync();
        }

        public async Task<List<LoadTracking>> GetTrackingsWithExceptionsAsync(bool unresolved = true)
        {
            var query = _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => lt.HasException);

            if (unresolved)
                query = query.Where(lt => lt.ExceptionResolvedAt == null);

            return await query
                .Include(lt => lt.Load)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.ExceptionReportedAt)
                .ToListAsync();
        }

        public async Task UpdateLocationAsync(long loadId, Location location, decimal? speed = null, int? heading = null, string updatedBy = "")
        {
            var latestTracking = await GetLatestTrackingAsync(loadId);

            if (latestTracking != null)
            {
                latestTracking.CurrentLocation = location;
                latestTracking.Speed = speed;
                latestTracking.Heading = heading;
                latestTracking.Timestamp = DateTime.UtcNow;
                latestTracking.UpdatedUser = updatedBy;
                latestTracking.UpdatedDate = DateTime.UtcNow;

                Update(latestTracking);
            }
        }

        public async Task UpdateStatusAsync(long loadId, TrackingStatus status, string? notes = null, string updatedBy = "")
        {
            var latestTracking = await GetLatestTrackingAsync(loadId);

            if (latestTracking != null)
            {
                latestTracking.Status = status;
                if (!string.IsNullOrEmpty(notes))
                    latestTracking.Notes = notes;
                latestTracking.Timestamp = DateTime.UtcNow;
                latestTracking.UpdatedUser = updatedBy;
                latestTracking.UpdatedDate = DateTime.UtcNow;

                Update(latestTracking);
            }
        }

        public async Task ReportExceptionAsync(long loadId, string exceptionType, string description, string updatedBy)
        {
            var latestTracking = await GetLatestTrackingAsync(loadId);

            if (latestTracking != null)
            {
                latestTracking.HasException = true;
                latestTracking.ExceptionType = exceptionType;
                latestTracking.ExceptionDescription = description;
                latestTracking.ExceptionReportedAt = DateTime.UtcNow;
                latestTracking.UpdatedUser = updatedBy;
                latestTracking.UpdatedDate = DateTime.UtcNow;

                Update(latestTracking);
            }
        }

        public async Task ResolveExceptionAsync(long trackingId, string updatedBy)
        {
            var tracking = await GetByIdAsync(trackingId);

            if (tracking != null && tracking.HasException)
            {
                tracking.ExceptionResolvedAt = DateTime.UtcNow;
                tracking.UpdatedUser = updatedBy;
                tracking.UpdatedDate = DateTime.UtcNow;

                Update(tracking);
            }
        }

        public async Task AddDigitalSignatureAsync(long loadId, string signature, string recipientName, string? recipientIdNumber = null, string updatedBy = "")
        {
            var latestTracking = await GetLatestTrackingAsync(loadId);

            if (latestTracking != null)
            {
                latestTracking.DigitalSignature = signature;
                latestTracking.RecipientName = recipientName;
                latestTracking.RecipientIdNumber = recipientIdNumber;
                latestTracking.UpdatedUser = updatedBy;
                latestTracking.UpdatedDate = DateTime.UtcNow;

                Update(latestTracking);
            }
        }

        public async Task AddDocumentationAsync(long loadId, List<string>? photoUrls = null, List<string>? documentUrls = null, string? notes = null, string updatedBy = "")
        {
            var latestTracking = await GetLatestTrackingAsync(loadId);

            if (latestTracking != null)
            {
                if (photoUrls != null && photoUrls.Any())
                {
                    var existingPhotos = latestTracking.PhotoUrls.ToList();
                    existingPhotos.AddRange(photoUrls);
                    latestTracking.PhotoUrls = existingPhotos;
                }

                if (documentUrls != null && documentUrls.Any())
                {
                    var existingDocs = latestTracking.DocumentUrls.ToList();
                    existingDocs.AddRange(documentUrls);
                    latestTracking.DocumentUrls = existingDocs;
                }

                if (!string.IsNullOrEmpty(notes))
                    latestTracking.Notes = notes;

                latestTracking.UpdatedUser = updatedBy;
                latestTracking.UpdatedDate = DateTime.UtcNow;

                Update(latestTracking);
            }
        }

        public async Task<List<LoadTracking>> GetDelayedLoadsAsync(int delayThresholdMinutes = 30)
        {
            return await _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => !lt.IsOnTime && lt.DelayMinutes >= delayThresholdMinutes)
                .Include(lt => lt.Load)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.DelayMinutes)
                .ToListAsync();
        }

        public async Task<List<LoadTracking>> GetOffRouteLoadsAsync(decimal deviationThresholdKm = 5.0m)
        {
            return await _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => !lt.IsOnRoute && lt.RouteDeviation >= deviationThresholdKm)
                .Include(lt => lt.Load)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.RouteDeviation)
                .ToListAsync();
        }

        public async Task<decimal> GetAverageDeliveryPerformanceAsync(long? driverId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.LoadTrackings
                .AsNoTracking()
                .Where(lt => lt.Status == TrackingStatus.Delivered);

            if (driverId.HasValue)
                query = query.Where(lt => lt.DriverId == driverId.Value);

            if (fromDate.HasValue)
                query = query.Where(lt => lt.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(lt => lt.Timestamp <= toDate.Value);

            var onTimeCount = await query.CountAsync(lt => lt.IsOnTime);
            var totalCount = await query.CountAsync();

            if (totalCount == 0)
                return 100m;

            return (decimal)onTimeCount / totalCount * 100m;
        }

        public async Task<List<LoadTracking>> SearchTrackingsAsync(
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
            int pageSize = 20)
        {
            var query = _context.LoadTrackings.AsNoTracking();

            if (status.HasValue)
                query = query.Where(lt => lt.Status == status.Value);

            if (loadId.HasValue)
                query = query.Where(lt => lt.LoadId == loadId.Value);

            if (driverId.HasValue)
                query = query.Where(lt => lt.DriverId == driverId.Value);

            if (matchId.HasValue)
                query = query.Where(lt => lt.MatchId == matchId.Value);

            if (hasException.HasValue)
                query = query.Where(lt => lt.HasException == hasException.Value);

            if (isOnTime.HasValue)
                query = query.Where(lt => lt.IsOnTime == isOnTime.Value);

            if (isOnRoute.HasValue)
                query = query.Where(lt => lt.IsOnRoute == isOnRoute.Value);

            if (fromDate.HasValue)
                query = query.Where(lt => lt.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(lt => lt.Timestamp <= toDate.Value);

            return await query
                .Include(lt => lt.Load)
                .Include(lt => lt.Driver)
                .Include(lt => lt.Match)
                .OrderByDescending(lt => lt.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
