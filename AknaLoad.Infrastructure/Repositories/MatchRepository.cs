using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Infrastructure.Persistence;
using AknaLoad.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AknaLoad.Infrastructure.Repositories
{
    public class MatchRepository : BaseRepository<Match>, IMatchRepository
    {
        public MatchRepository(AknaLoadDbContext context) : base(context)
        {
        }

        public async Task<List<Match>> GetMatchesForLoadAsync(long loadId, MatchStatus? status = null)
        {
            var query = _dbSet.Where(m => m.LoadId == loadId);

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            return await query
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .OrderByDescending(m => m.MatchScore)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Match>> GetMatchesForDriverAsync(long driverId, MatchStatus? status = null)
        {
            var query = _dbSet.Where(m => m.DriverId == driverId);

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            return await query
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .OrderByDescending(m => m.ProposedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Match?> GetMatchByCodeAsync(string matchCode)
        {
            return await _dbSet
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .FirstOrDefaultAsync(m => m.MatchCode == matchCode);
        }

        public async Task<List<Match>> GetPendingMatchesAsync(DateTime? beforeExpiry = null)
        {
            var query = _dbSet.Where(m => m.Status == MatchStatus.Proposed || m.Status == MatchStatus.DriverNotified);

            if (beforeExpiry.HasValue)
                query = query.Where(m => m.ExpiresAt <= beforeExpiry.Value);

            return await query
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .OrderBy(m => m.ExpiresAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Match>> GetExpiredMatchesAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(m => m.ExpiresAt <= now &&
                           (m.Status == MatchStatus.Proposed || m.Status == MatchStatus.DriverNotified))
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateMatchStatusAsync(long matchId, MatchStatus status, string updatedBy, string? rejectionReason = null)
        {
            var match = await GetByIdAsync(matchId);
            if (match != null)
            {
                match.Status = status;
                match.UpdatedUser = updatedBy;
                match.RespondedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(rejectionReason))
                    match.RejectionReason = rejectionReason;

                Update(match);
            }
        }

        public async Task<Match?> GetActiveMatchByLoadAsync(long loadId)
        {
            return await _dbSet
                .Where(m => m.LoadId == loadId &&
                           (m.Status == MatchStatus.DriverAccepted || m.Status == MatchStatus.Confirmed))
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .FirstOrDefaultAsync();
        }

        public async Task<Match?> GetActiveMatchByDriverAsync(long driverId)
        {
            return await _dbSet
                .Where(m => m.DriverId == driverId &&
                           (m.Status == MatchStatus.DriverAccepted || m.Status == MatchStatus.Confirmed))
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Match>> GetCompletedMatchesAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            long? driverId = null,
            long? loadId = null)
        {
            var query = _dbSet.Where(m => m.Status == MatchStatus.Confirmed && m.ActualDeliveryTime.HasValue);

            if (fromDate.HasValue)
                query = query.Where(m => m.ActualDeliveryTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(m => m.ActualDeliveryTime <= toDate.Value);

            if (driverId.HasValue)
                query = query.Where(m => m.DriverId == driverId.Value);

            if (loadId.HasValue)
                query = query.Where(m => m.LoadId == loadId.Value);

            return await query
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .OrderByDescending(m => m.ActualDeliveryTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateMatchRatingAsync(
            long matchId,
            decimal? loadOwnerRating = null,
            string? loadOwnerFeedback = null,
            decimal? driverRating = null,
            string? driverFeedback = null,
            string updatedBy = "")
        {
            var match = await GetByIdAsync(matchId);
            if (match != null)
            {
                if (loadOwnerRating.HasValue)
                    match.LoadOwnerRating = loadOwnerRating.Value;

                if (!string.IsNullOrEmpty(loadOwnerFeedback))
                    match.LoadOwnerFeedback = loadOwnerFeedback;

                if (driverRating.HasValue)
                    match.DriverRating = driverRating.Value;

                if (!string.IsNullOrEmpty(driverFeedback))
                    match.DriverFeedback = driverFeedback;

                match.UpdatedUser = updatedBy;
                Update(match);
            }
        }

        public async Task<decimal> GetAverageMatchScoreAsync(long? driverId = null, long? loadId = null)
        {
            var query = _dbSet.Where(m => m.Status == MatchStatus.Confirmed);

            if (driverId.HasValue)
                query = query.Where(m => m.DriverId == driverId.Value);

            if (loadId.HasValue)
                query = query.Where(m => m.LoadId == loadId.Value);

            var matches = await query.ToListAsync();

            if (!matches.Any())
                return 0;

            return matches.Average(m => m.MatchScore);
        }

        public async Task<List<Match>> GetMatchesWithEmergencyAsync()
        {
            return await _dbSet
                .Where(m => m.HasEmergency)
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .OrderByDescending(m => m.EmergencyReportedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ReportEmergencyAsync(long matchId, string description, string updatedBy)
        {
            var match = await GetByIdAsync(matchId);
            if (match != null)
            {
                match.HasEmergency = true;
                match.EmergencyDescription = description;
                match.EmergencyReportedAt = DateTime.UtcNow;
                match.UpdatedUser = updatedBy;
                Update(match);
            }
        }

        public async Task<List<Match>> SearchMatchesAsync(
            MatchStatus? status = null,
            long? loadId = null,
            long? driverId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            decimal? minMatchScore = null,
            bool? hasEmergency = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            var query = _dbSet.AsQueryable();

            if (status.HasValue)
                query = query.Where(m => m.Status == status.Value);

            if (loadId.HasValue)
                query = query.Where(m => m.LoadId == loadId.Value);

            if (driverId.HasValue)
                query = query.Where(m => m.DriverId == driverId.Value);

            if (fromDate.HasValue)
                query = query.Where(m => m.ProposedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(m => m.ProposedAt <= toDate.Value);

            if (minMatchScore.HasValue)
                query = query.Where(m => m.MatchScore >= minMatchScore.Value);

            if (hasEmergency.HasValue)
                query = query.Where(m => m.HasEmergency == hasEmergency.Value);

            return await query
                .Include(m => m.Driver)
                .Include(m => m.Load)
                .OrderByDescending(m => m.ProposedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}