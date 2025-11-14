using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Services
{
    public interface ILoadService
    {
        /// <summary>
        /// Create a new load (single or multi-stop)
        /// </summary>
        Task<Load> CreateLoadAsync(Load load, string createdBy);

        /// <summary>
        /// Get load by ID
        /// </summary>
        Task<Load?> GetLoadByIdAsync(long id);

        /// <summary>
        /// Get load by load code
        /// </summary>
        Task<Load?> GetLoadByCodeAsync(string loadCode);

        /// <summary>
        /// Get loads by company ID with filters
        /// </summary>
        Task<List<Load>> GetLoadsByCompanyIdAsync(
            long companyId,
            LoadStatus? status = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null);

        /// <summary>
        /// Get paged loads with filtering
        /// </summary>
        Task<(List<Load> Items, int TotalCount)> GetPagedLoadsAsync(
            long? companyId = null,
            LoadStatus? status = null,
            List<LoadStatus>? statuses = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            DateTime? pickupFrom = null,
            DateTime? pickupTo = null,
            DateTime? deliveryFrom = null,
            DateTime? deliveryTo = null,
            LoadType? loadType = null,
            bool? isMultiStop = null,
            string? originCity = null,
            string? destinationCity = null,
            int pageNumber = 1,
            int pageSize = 20,
            string sortBy = "CreatedAt",
            bool sortDescending = true);

        /// <summary>
        /// Update load
        /// </summary>
        Task<Load> UpdateLoadAsync(Load load, string updatedBy);

        /// <summary>
        /// Delete load (soft delete)
        /// </summary>
        Task<bool> DeleteLoadAsync(long id, string deletedBy);

        /// <summary>
        /// Publish load (make it available for matching)
        /// </summary>
        Task<Load> PublishLoadAsync(long id, string publishedBy);

        /// <summary>
        /// Cancel load
        /// </summary>
        Task<Load> CancelLoadAsync(long id, string cancelledBy, string? reason = null);

        /// <summary>
        /// Validate load data
        /// </summary>
        (bool IsValid, List<string> Errors) ValidateLoad(Load load);
    }
}