using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;

namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface ILoadRepository : IBaseRepository<Load>
    {
        /// <summary>
        /// Get loads by company ID with optional filtering
        /// </summary>
        Task<List<Load>> GetByCompanyIdAsync(
            long companyId,
            LoadStatus? status = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            bool trackChanges = true);

        /// <summary>
        /// Get paged loads with filtering
        /// </summary>
        Task<(List<Load> Items, int TotalCount)> GetPagedAsync(
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
        /// Get load by ID with all related entities
        /// </summary>
        Task<Load?> GetByIdWithDetailsAsync(long id);

        /// <summary>
        /// Get load by load code
        /// </summary>
        Task<Load?> GetByLoadCodeAsync(string loadCode);

        /// <summary>
        /// Check if load code exists
        /// </summary>
        Task<bool> IsLoadCodeUniqueAsync(string loadCode);

        /// <summary>
        /// Get loads by status
        /// </summary>
        Task<List<Load>> GetByStatusAsync(LoadStatus status, bool trackChanges = true);
    }
}