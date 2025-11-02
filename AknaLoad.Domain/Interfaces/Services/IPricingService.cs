using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Interfaces.Services
{
    public interface IPricingService
    {
        Task<PricingCalculation> CalculatePriceAsync(Load load, string algorithmVersion = "v1.0");
        Task<decimal> GetBasePriceAsync(Load load);
        Task<decimal> ApplyDistanceFactorAsync(decimal basePrice, decimal distanceKm);
        Task<decimal> ApplyWeightFactorAsync(decimal basePrice, decimal weight);
        Task<decimal> ApplyVolumeFactorAsync(decimal basePrice, decimal? volume);
        Task<decimal> ApplyUrgencyFactorAsync(decimal basePrice, DateTime pickupDateTime, DateTime deliveryDeadline);
        Task<decimal> ApplyDemandFactorAsync(decimal basePrice, string pickupCity, string deliveryCity, DateTime pickupDateTime);
        Task<decimal> ApplySeasonalFactorAsync(decimal basePrice, DateTime pickupDateTime);
        Task<decimal> ApplySpecialRequirementsFactorAsync(decimal basePrice, List<SpecialRequirement> requirements);
        Task<decimal> CalculateFuelCostAsync(decimal distanceKm, LoadType loadType);
        Task<decimal> CalculateTollCostAsync(decimal distanceKm, string routeType);
        Task<decimal> GetMarketDemandFactorAsync(string pickupCity, string deliveryCity, DateTime dateTime);
        Task<List<decimal>> GetCompetitorPricesAsync(Load load);
        Task<decimal> GetAverageMarketPriceAsync(LoadType loadType, decimal distanceKm, decimal weight);
        Task<bool> AcceptCalculationAsync(long calculationId, decimal agreedPrice, string acceptedBy);
        Task<PricingCalculation?> GetLatestCalculationAsync(long loadId);
        Task<List<PricingCalculation>> GetCalculationHistoryAsync(long loadId);
        Task<decimal> GetAcceptanceRateAsync(string? algorithmVersion = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<decimal> GetAveragePriceVarianceAsync(string? algorithmVersion = null);
        Task<PricingCalculation> AdjustPriceManuallyAsync(long calculationId, decimal adjustment, string reason, string adjustedBy);
        Task<Dictionary<string, decimal>> GetPricingFactorsAsync(Load load);
        Task UpdateAlgorithmParametersAsync(string algorithmVersion, Dictionary<string, decimal> parameters);
    }
}