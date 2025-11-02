using AknaLoad.Domain.Dtos;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Services
{
    public interface IMatchingService
    {
        Task<List<Match>> FindMatchesForLoadAsync(long loadId, int maxMatches = 10);
        Task<List<Match>> FindMatchesForDriverAsync(long driverId, int maxMatches = 10);
        Task<Match> CreateMatchAsync(long loadId, long driverId, long vehicleId, string createdBy);
        Task<bool> AcceptMatchAsync(long matchId, string acceptedBy);
        Task<bool> RejectMatchAsync(long matchId, string reason, string rejectedBy);
        Task<Match?> GetMatchByIdAsync(long matchId);
        Task<Match?> GetMatchByCodeAsync(string matchCode);
        Task<List<Match>> GetMatchesForLoadAsync(long loadId, MatchStatus? status = null);
        Task<List<Match>> GetMatchesForDriverAsync(long driverId, MatchStatus? status = null);
        Task<bool> ExpireMatchAsync(long matchId, string expiredBy);
        Task<List<Match>> GetExpiredMatchesAsync();
        Task<decimal> CalculateMatchScoreAsync(Load load, Driver driver, VehicleDto vehicle);
        Task<string> GenerateMatchCodeAsync();
        Task<bool> NotifyDriverAsync(long matchId);
        Task<bool> ConfirmMatchAsync(long matchId, string confirmedBy);
        Task<bool> CancelMatchAsync(long matchId, string reason, string cancelledBy);
        Task<List<Match>> GetPendingMatchesAsync(long? driverId = null);
        Task ProcessExpiredMatchesAsync();
        Task<Match?> GetActiveMatchByLoadAsync(long loadId);
        Task<Match?> GetActiveMatchByDriverAsync(long driverId);
    }
}