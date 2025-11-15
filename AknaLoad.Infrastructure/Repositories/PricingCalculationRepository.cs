using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Infrastructure.Persistence;
using AknaLoad.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AknaLoad.Infrastructure.Repositories
{
    public class PricingCalculationRepository : BaseRepository<PricingCalculation>, IPricingCalculationRepository
    {
        private readonly AknaLoadDbContext _context;

        public PricingCalculationRepository(AknaLoadDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Override GetByIdAsync to include Load navigation property
        /// </summary>
        public new async Task<PricingCalculation?> GetByIdAsync(long id, bool trackChanges = true)
        {
            var query = trackChanges ? _context.PricingCalculations.AsTracking() : _context.PricingCalculations.AsNoTracking();

            return await query
                .Include(p => p.Load)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}