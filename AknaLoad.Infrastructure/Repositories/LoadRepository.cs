using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using AknaLoad.Infrastructure.Persistence;

namespace AknaLoad.Infrastructure.Repositories
{
    public class LoadRepository : BaseRepository<Load>, ILoadRepository
    {
        private readonly AknaLoadDbContext _context;

        public LoadRepository(AknaLoadDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Override GetByIdAsync to include all navigation properties (LoadStops, Matches, PricingCalculations, LoadTrackings)
        /// </summary>
        public new async Task<Load?> GetByIdAsync(long id, bool trackChanges = true)
        {
            var query = trackChanges ? _context.Loads.AsTracking() : _context.Loads.AsNoTracking();

            return await query
                .Include(l => l.LoadStops.OrderBy(s => s.StopOrder))
                .Include(l => l.Matches)
                .Include(l => l.PricingCalculations)
                .Include(l => l.LoadTrackings)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<Load>> GetByCompanyIdAsync(
            long companyId,
            LoadStatus? status = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            bool trackChanges = true)
        {
            var query = trackChanges ? _context.Loads.AsTracking() : _context.Loads.AsNoTracking();

            query = query.Where(l => l.CompanyId == companyId);

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);

            if (createdFrom.HasValue)
                query = query.Where(l => l.CreatedDate >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(l => l.CreatedDate <= createdTo.Value);

            return await query
                .Include(l => l.LoadStops)
                .Include(l => l.Matches)
                .Include(l => l.PricingCalculations)
                .Include(l => l.LoadTrackings)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }

        public async Task<(List<Load> Items, int TotalCount)> GetPagedAsync(
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
            bool sortDescending = true)
        {
            var query = _context.Loads.AsNoTracking();

            // Apply filters
            if (companyId.HasValue)
                query = query.Where(l => l.CompanyId == companyId.Value);

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);

            if (statuses != null && statuses.Any())
                query = query.Where(l => statuses.Contains(l.Status));

            if (createdFrom.HasValue)
                query = query.Where(l => l.CreatedDate >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(l => l.CreatedDate <= createdTo.Value);

            if (pickupFrom.HasValue)
                query = query.Where(l => l.EarliestPickupTime >= pickupFrom.Value);

            if (pickupTo.HasValue)
                query = query.Where(l => l.EarliestPickupTime <= pickupTo.Value);

            if (deliveryFrom.HasValue)
                query = query.Where(l => l.LatestDeliveryTime >= deliveryFrom.Value);

            if (deliveryTo.HasValue)
                query = query.Where(l => l.LatestDeliveryTime <= deliveryTo.Value);

            if (loadType.HasValue)
                query = query.Where(l => l.LoadType == loadType.Value);

            if (isMultiStop.HasValue)
                query = query.Where(l => l.IsMultiStop == isMultiStop.Value);

            // City filters - using LoadStops
            if (!string.IsNullOrEmpty(originCity))
            {
                query = query.Where(l => l.LoadStops
                    .Any(s => s.StopOrder == 1 && s.LocationJson.Contains(originCity)));
            }

            if (!string.IsNullOrEmpty(destinationCity))
            {
                query = query.Where(l => l.LoadStops
                    .Where(s => s.StopOrder == l.LoadStops.Max(stop => stop.StopOrder))
                    .Any(s => s.LocationJson.Contains(destinationCity)));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "title" => sortDescending ? query.OrderByDescending(l => l.Title) : query.OrderBy(l => l.Title),
                "status" => sortDescending ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
                "weight" => sortDescending ? query.OrderByDescending(l => l.Weight) : query.OrderBy(l => l.Weight),
                "price" => sortDescending ? query.OrderByDescending(l => l.FixedPrice) : query.OrderBy(l => l.FixedPrice),
                "publishedat" => sortDescending ? query.OrderByDescending(l => l.PublishedAt) : query.OrderBy(l => l.PublishedAt),
                _ => sortDescending ? query.OrderByDescending(l => l.CreatedDate) : query.OrderBy(l => l.CreatedDate),
            };

            // Apply pagination
            var items = await query
                .Include(l => l.LoadStops)
                .Include(l => l.Matches)
                .Include(l => l.PricingCalculations)
                .Include(l => l.LoadTrackings)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Load?> GetByIdWithDetailsAsync(long id)
        {
            return await _context.Loads
                .AsNoTracking()
                .Include(l => l.LoadStops.OrderBy(s => s.StopOrder))
                .Include(l => l.Matches)
                .Include(l => l.PricingCalculations)
                .Include(l => l.LoadTrackings)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Load?> GetByLoadCodeAsync(string loadCode)
        {
            return await _context.Loads
                .AsNoTracking()
                .Include(l => l.LoadStops)
                .Include(l => l.Matches)
                .Include(l => l.PricingCalculations)
                .Include(l => l.LoadTrackings)
                .FirstOrDefaultAsync(l => l.LoadCode == loadCode);
        }

        public async Task<bool> IsLoadCodeUniqueAsync(string loadCode)
        {
            return !await _context.Loads.AnyAsync(l => l.LoadCode == loadCode);
        }

        public async Task<List<Load>> GetByStatusAsync(LoadStatus status, bool trackChanges = true)
        {
            var query = trackChanges ? _context.Loads.AsTracking() : _context.Loads.AsNoTracking();

            return await query
                .Where(l => l.Status == status)
                .Include(l => l.LoadStops)
                .Include(l => l.Matches)
                .Include(l => l.PricingCalculations)
                .Include(l => l.LoadTrackings)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }
    }
}