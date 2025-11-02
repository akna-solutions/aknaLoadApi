using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Infrastructure.Persistence;
using AknaLoad.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AknaLoad.Infrastructure.Repositories
{
    public class PricingCalculationRepository : BaseRepository<PricingCalculation>, IPricingCalculationRepository
    {
        public PricingCalculationRepository(AknaLoadDbContext context) : base(context)
        {
        }

        public async Task<PricingCalculation?> GetLatestCalculationForLoadAsync(long loadId)
        {
            return await _dbSet
                .Where(pc => pc.LoadId == loadId)
                .OrderByDescending(pc => pc.CalculatedAt)
                .Include(pc => pc.Load)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PricingCalculation>> GetCalculationHistoryAsync(long loadId)
        {
            return await _dbSet
                .Where(pc => pc.LoadId == loadId)
                .OrderByDescending(pc => pc.CalculatedAt)
                .Include(pc => pc.Load)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PricingCalculation>> GetCalculationsByAlgorithmVersionAsync(string algorithmVersion)
        {
            return await _dbSet
                .Where(pc => pc.AlgorithmVersion == algorithmVersion)
                .Include(pc => pc.Load)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<decimal> GetAverageCalculatedPriceAsync(
            string? algorithmVersion = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(algorithmVersion))
                query = query.Where(pc => pc.AlgorithmVersion == algorithmVersion);

            if (fromDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt <= toDate.Value);

            var calculations = await query.ToListAsync();

            if (!calculations.Any())
                return 0;

            return calculations.Average(pc => pc.CalculatedPrice);
        }

        public async Task<decimal> GetAcceptanceRateAsync(
            string? algorithmVersion = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(algorithmVersion))
                query = query.Where(pc => pc.AlgorithmVersion == algorithmVersion);

            if (fromDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt <= toDate.Value);

            var totalCalculations = await query.CountAsync();
            var acceptedCalculations = await query.CountAsync(pc => pc.WasAccepted);

            if (totalCalculations == 0)
                return 0;

            return (decimal)acceptedCalculations / totalCalculations * 100;
        }

        public async Task<List<PricingCalculation>> GetAcceptedCalculationsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _dbSet.Where(pc => pc.WasAccepted);

            if (fromDate.HasValue)
                query = query.Where(pc => pc.AcceptedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pc => pc.AcceptedAt <= toDate.Value);

            return await query
                .Include(pc => pc.Load)
                .OrderByDescending(pc => pc.AcceptedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<PricingCalculation>> GetCalculationsWithVarianceAsync(
            decimal minVariancePercentage,
            decimal maxVariancePercentage)
        {
            return await _dbSet
                .Where(pc => pc.PriceVariancePercentage.HasValue &&
                            pc.PriceVariancePercentage.Value >= minVariancePercentage &&
                            pc.PriceVariancePercentage.Value <= maxVariancePercentage)
                .Include(pc => pc.Load)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task MarkAsAcceptedAsync(long calculationId, decimal agreedPrice, string updatedBy)
        {
            var calculation = await GetByIdAsync(calculationId);
            if (calculation != null)
            {
                calculation.WasAccepted = true;
                calculation.AcceptedAt = DateTime.UtcNow;
                calculation.FinalAgreedPrice = agreedPrice;

                if (calculation.CalculatedPrice > 0)
                {
                    calculation.PriceVariancePercentage =
                        ((agreedPrice - calculation.CalculatedPrice) / calculation.CalculatedPrice) * 100;
                }

                calculation.UpdatedUser = updatedBy;
                Update(calculation);
            }
        }

        public async Task<decimal> GetAveragePriceVarianceAsync(string? algorithmVersion = null)
        {
            var query = _dbSet.Where(pc => pc.WasAccepted && pc.PriceVariancePercentage.HasValue);

            if (!string.IsNullOrEmpty(algorithmVersion))
                query = query.Where(pc => pc.AlgorithmVersion == algorithmVersion);

            var calculations = await query.ToListAsync();

            if (!calculations.Any())
                return 0;

            return calculations.Average(pc => pc.PriceVariancePercentage!.Value);
        }

        public async Task<List<PricingCalculation>> GetManuallyAdjustedCalculationsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _dbSet.Where(pc => pc.IsManuallyAdjusted);

            if (fromDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt <= toDate.Value);

            return await query
                .Include(pc => pc.Load)
                .OrderByDescending(pc => pc.CalculatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetMarketDemandStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pc => pc.CalculatedAt <= toDate.Value);

            var calculations = await query.ToListAsync();

            var stats = calculations
                .GroupBy(pc => pc.MarketDemand)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(pc => pc.CalculatedPrice)
                );

            return stats;
        }

        public async Task<List<PricingCalculation>> GetCalculationsForPerformanceAnalysisAsync(
            string algorithmVersion,
            int limit = 1000)
        {
            return await _dbSet
                .Where(pc => pc.AlgorithmVersion == algorithmVersion && pc.WasAccepted)
                .OrderByDescending(pc => pc.CalculatedAt)
                .Take(limit)
                .Include(pc => pc.Load)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}