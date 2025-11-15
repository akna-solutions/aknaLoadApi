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
        private readonly AknaLoadDbContext _context;

        public MatchRepository(AknaLoadDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Override GetByIdAsync to include navigation properties (Load, Driver)
        /// </summary>
        public new async Task<Match?> GetByIdAsync(long id, bool trackChanges = true)
        {
            var query = trackChanges ? _context.Matches.AsTracking() : _context.Matches.AsNoTracking();

            return await query
                .Include(m => m.Load)
                    .ThenInclude(l => l.LoadStops.OrderBy(s => s.StopOrder))
                .Include(m => m.Driver)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}