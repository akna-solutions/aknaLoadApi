using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Services
{
    public interface ILoadService
    {
        Task<Load> CreateLoadAsync(Load load, string createdBy);
        Task<Load> UpdateLoadAsync(Load load, string updatedBy);
        Task<bool> DeleteLoadAsync(long loadId, string deletedBy);
        Task<Load?> GetLoadByIdAsync(long loadId);
        Task<Load?> GetLoadByCodeAsync(string loadCode);
        Task<List<Load>> GetLoadsByOwnerAsync(long ownerId, LoadStatus? status = null);
        Task<List<Load>> GetAvailableLoadsAsync(Location? driverLocation = null, int maxDistanceKm = 500);
        Task<bool> PublishLoadAsync(long loadId, string publishedBy);
        Task<bool> CancelLoadAsync(long loadId, string reason, string cancelledBy);
        Task<decimal?> CalculateLoadPriceAsync(Load load);
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
        Task<string> GenerateLoadCodeAsync();
        Task<bool> ValidateLoadAsync(Load load);
        Task<List<Load>> GetExpiringLoadsAsync(int hoursAhead = 24);
        Task<bool> ExtendDeadlineAsync(long loadId, DateTime newDeadline, string updatedBy);
    }
}