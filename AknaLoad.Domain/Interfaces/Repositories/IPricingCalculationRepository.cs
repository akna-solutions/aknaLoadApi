using AknaLoad.Domain.Entities;

namespace AknaLoad.Domain.Interfaces.Repositories
{
    public interface IPricingCalculationRepository : IBaseRepository<PricingCalculation>
    {
        Task<PricingCalculation?> GetLatestCalculationForLoadAsync(long loadId);

        Task<List<PricingCalculation>> GetCalculationHistoryAsync(long loadId);

        Task<List<PricingCalculation>> GetCalculationsByAlgorithmVersionAsync(string algorithmVersion);

        Task<decimal> GetAverageCalculatedPriceAsync(
            string? algorithmVersion = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<decimal> GetAcceptanceRateAsync(
            string? algorithmVersion = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<List<PricingCalculation>> GetAcceptedCalculationsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<List<PricingCalculation>> GetCalculationsWithVarianceAsync(
            decimal minVariancePercentage,
            decimal maxVariancePercentage);

        Task MarkAsAcceptedAsync(long calculationId, decimal agreedPrice, string updatedBy);

        Task<decimal> GetAveragePriceVarianceAsync(string? algorithmVersion = null);

        Task<List<PricingCalculation>> GetManuallyAdjustedCalculationsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<Dictionary<string, decimal>> GetMarketDemandStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        Task<List<PricingCalculation>> GetCalculationsForPerformanceAnalysisAsync(
            string algorithmVersion,
            int limit = 1000);
    }
}