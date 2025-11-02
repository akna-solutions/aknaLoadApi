using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;

namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface IMatchRepository : IBaseRepository<Match>
    {
        Task<List<Match>> GetMatchesForLoadAsync(long loadId, MatchStatus? status = null);

        Task<List<Match>> GetMatchesForDriverAsync(long driverId, MatchStatus? status = null);

        Task<Match?> GetMatchByCodeAsync(string matchCode);

        Task<List<Match>> GetPendingMatchesAsync(DateTime? beforeExpiry = null);

        Task<List<Match>> GetExpiredMatchesAsync();

        Task UpdateMatchStatusAsync(long matchId, MatchStatus status, string updatedBy, string? rejectionReason = null);

        Task<Match?> GetActiveMatchByLoadAsync(long loadId);

        Task<Match?> GetActiveMatchByDriverAsync(long driverId);

        Task<List<Match>> GetCompletedMatchesAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            long? driverId = null,
            long? loadId = null);

        Task UpdateMatchRatingAsync(
            long matchId,
            decimal? loadOwnerRating = null,
            string? loadOwnerFeedback = null,
            decimal? driverRating = null,
            string? driverFeedback = null,
            string updatedBy = "");

        Task<decimal> GetAverageMatchScoreAsync(long? driverId = null, long? loadId = null);

        Task<List<Match>> GetMatchesWithEmergencyAsync();

        Task ReportEmergencyAsync(long matchId, string description, string updatedBy);

        Task<List<Match>> SearchMatchesAsync(
            MatchStatus? status = null,
            long? loadId = null,
            long? driverId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            decimal? minMatchScore = null,
            bool? hasEmergency = null,
            int pageNumber = 1,
            int pageSize = 20);
    }
}