using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;

namespace AknaLoad.Domain.Interfaces.Services
{
    public interface IPricingService
    {
        // Note: PricingResult and VehicleMatch are defined in AknaLoad.Application.Services
        // These methods return Task<object> to avoid circular dependencies
        // Cast to appropriate types in the calling code

        Task<object> CalculatePriceAsync(
            decimal distanceKm,
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements,
            DateTime pickupDateTime,
            DateTime deliveryDeadline,
            bool useAIOptimization = true);

        Task<object> GetVehicleMatchesAsync(
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements,
            decimal? distanceKm = null,
            decimal? length = null,
            decimal? width = null,
            decimal? height = null,
            bool useAIRecommendation = true);

        Task<PricingCalculation> SavePricingCalculationAsync(
            long loadId,
            object pricingResult);
    }
}