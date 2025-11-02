using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Domain.Interfaces.Services;
using AknaLoad.Domain.Interfaces.UnitOfWorks;

namespace AknaLoad.Application.Services
{
    public class PricingService : IPricingService
    {
        private readonly IPricingCalculationRepository _pricingRepository;
        private readonly ILoadRepository _loadRepository;
        private readonly IUnitOfWork _unitOfWork;

        // Base pricing parameters (these could be moved to configuration)
        private const decimal BasePricePerKm = 1.5m;
        private const decimal BasePricePerKg = 0.02m;
        private const decimal BasePricePerM3 = 15.0m;
        private const decimal MinimumPrice = 100.0m;

        public PricingService(
            IPricingCalculationRepository pricingRepository,
            ILoadRepository loadRepository,
            IUnitOfWork unitOfWork)
        {
            _pricingRepository = pricingRepository;
            _loadRepository = loadRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PricingCalculation> CalculatePriceAsync(Load load, string algorithmVersion = "v1.0")
        {
            // Step 1: Calculate base price
            var basePrice = await GetBasePriceAsync(load);

            // Step 2: Apply various factors
            var priceWithDistance = await ApplyDistanceFactorAsync(basePrice, load.DistanceKm ?? 0);
            var priceWithWeight = await ApplyWeightFactorAsync(priceWithDistance, load.Weight);
            var priceWithVolume = await ApplyVolumeFactorAsync(priceWithWeight, load.Volume);
            var priceWithUrgency = await ApplyUrgencyFactorAsync(priceWithVolume, load.PickupDateTime, load.DeliveryDeadline);
            var priceWithDemand = await ApplyDemandFactorAsync(priceWithUrgency,
                load.PickupLocation?.City ?? "", load.DeliveryLocation?.City ?? "", load.PickupDateTime);
            var priceWithSeason = await ApplySeasonalFactorAsync(priceWithDemand, load.PickupDateTime);
            var finalPrice = await ApplySpecialRequirementsFactorAsync(priceWithSeason, load.SpecialRequirements);

            // Step 3: Calculate cost estimates
            var fuelCost = await CalculateFuelCostAsync(load.DistanceKm ?? 0, load.LoadType);
            var tollCost = await CalculateTollCostAsync(load.DistanceKm ?? 0, "highway");

            // Step 4: Get market data
            var competitorPrices = await GetCompetitorPricesAsync(load);
            var averageMarketPrice = await GetAverageMarketPriceAsync(load.LoadType, load.DistanceKm ?? 0, load.Weight);
            var demandFactor = await GetMarketDemandFactorAsync(
                load.PickupLocation?.City ?? "", load.DeliveryLocation?.City ?? "", load.PickupDateTime);

            // Step 5: Create pricing calculation record
            var calculation = new PricingCalculation
            {
                LoadId = load.Id,
                AlgorithmVersion = algorithmVersion,
                CalculatedPrice = Math.Max(finalPrice, MinimumPrice),

                // Base components
                BasePrice = basePrice,
                DistanceFactor = CalculateDistanceFactor(load.DistanceKm ?? 0),
                WeightFactor = CalculateWeightFactor(load.Weight),
                VolumeFactor = CalculateVolumeFactor(load.Volume),
                UrgencyFactor = CalculateUrgencyFactor(load.PickupDateTime, load.DeliveryDeadline),
                DemandFactor = demandFactor,
                SeasonalFactor = CalculateSeasonalFactor(load.PickupDateTime),
                SpecialRequirementsFactor = CalculateSpecialRequirementsFactor(load.SpecialRequirements),

                // Cost estimates
                FuelCostEstimate = fuelCost,
                TollCostEstimate = tollCost,
                DriverCostEstimate = CalculateDriverCost(load.DistanceKm ?? 0),
                VehicleCostEstimate = CalculateVehicleCost(load.LoadType),

                // Market data
                CompetitorPricesJson = System.Text.Json.JsonSerializer.Serialize(competitorPrices),
                MarketDemand = GetMarketDemandLevel(demandFactor),
                AvailableDriversCount = await GetAvailableDriversCount(load),
                AverageMarketPrice = averageMarketPrice,

                // Time factors
                TimeOfDayFactor = CalculateTimeOfDayFactor(load.PickupDateTime),
                DayOfWeekFactor = CalculateDayOfWeekFactor(load.PickupDateTime),
                RouteDifficultyFactor = 1.0m, // Would be calculated based on route analysis
                ReturnLoadProbability = 0.6m, // Would be calculated based on historical data

                CalculatedAt = DateTime.UtcNow,
                CalculationInputsJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    LoadId = load.Id,
                    Distance = load.DistanceKm,
                    Weight = load.Weight,
                    Volume = load.Volume,
                    LoadType = load.LoadType.ToString(),
                    PickupCity = load.PickupLocation?.City,
                    DeliveryCity = load.DeliveryLocation?.City,
                    PickupDateTime = load.PickupDateTime,
                    DeliveryDeadline = load.DeliveryDeadline,
                    SpecialRequirements = load.SpecialRequirements
                })
            };

            await _pricingRepository.AddAsync(calculation);
            await _unitOfWork.SaveChangesAsync();

            return calculation;
        }

        public async Task<decimal> GetBasePriceAsync(Load load)
        {
            var distancePrice = (load.DistanceKm ?? 0) * BasePricePerKm;
            var weightPrice = load.Weight * BasePricePerKg;
            var volumePrice = (load.Volume ?? 0) * BasePricePerM3;

            return Math.Max(distancePrice + weightPrice + volumePrice, MinimumPrice);
        }

        public async Task<decimal> ApplyDistanceFactorAsync(decimal basePrice, decimal distanceKm)
        {
            var factor = CalculateDistanceFactor(distanceKm);
            return basePrice * factor;
        }

        public async Task<decimal> ApplyWeightFactorAsync(decimal basePrice, decimal weight)
        {
            var factor = CalculateWeightFactor(weight);
            return basePrice * factor;
        }

        public async Task<decimal> ApplyVolumeFactorAsync(decimal basePrice, decimal? volume)
        {
            var factor = CalculateVolumeFactor(volume);
            return basePrice * factor;
        }

        public async Task<decimal> ApplyUrgencyFactorAsync(decimal basePrice, DateTime pickupDateTime, DateTime deliveryDeadline)
        {
            var factor = CalculateUrgencyFactor(pickupDateTime, deliveryDeadline);
            return basePrice * factor;
        }

        public async Task<decimal> ApplyDemandFactorAsync(decimal basePrice, string pickupCity, string deliveryCity, DateTime pickupDateTime)
        {
            var factor = await GetMarketDemandFactorAsync(pickupCity, deliveryCity, pickupDateTime);
            return basePrice * factor;
        }

        public async Task<decimal> ApplySeasonalFactorAsync(decimal basePrice, DateTime pickupDateTime)
        {
            var factor = CalculateSeasonalFactor(pickupDateTime);
            return basePrice * factor;
        }

        public async Task<decimal> ApplySpecialRequirementsFactorAsync(decimal basePrice, List<SpecialRequirement> requirements)
        {
            var factor = CalculateSpecialRequirementsFactor(requirements);
            return basePrice * factor;
        }

        public async Task<decimal> CalculateFuelCostAsync(decimal distanceKm, LoadType loadType)
        {
            // Fuel consumption varies by vehicle type and load
            decimal fuelConsumptionPer100Km = loadType switch
            {
                LoadType.GeneralCargo => 25.0m,
                LoadType.Hazardous => 30.0m,
                LoadType.Refrigerated => 35.0m,
                LoadType.Oversized => 40.0m,
                _ => 25.0m
            };

            decimal fuelPricePerLiter = 8.5m; // Current diesel price in Turkey
            return (distanceKm / 100) * fuelConsumptionPer100Km * fuelPricePerLiter;
        }

        public async Task<decimal> CalculateTollCostAsync(decimal distanceKm, string routeType)
        {
            // Simplified toll calculation - would use actual route analysis in production
            decimal tollPricePerKm = routeType.ToLower() switch
            {
                "highway" => 0.15m,
                "bridge" => 0.25m,
                "city" => 0.05m,
                _ => 0.10m
            };

            return distanceKm * tollPricePerKm;
        }

        public async Task<decimal> GetMarketDemandFactorAsync(string pickupCity, string deliveryCity, DateTime dateTime)
        {
            // Simplified demand calculation - would use real market data in production
            var baseMultiplier = 1.0m;

            // High-demand routes
            if (IsHighDemandRoute(pickupCity, deliveryCity))
                baseMultiplier += 0.15m;

            // Peak hours (morning and evening)
            var hour = dateTime.Hour;
            if ((hour >= 7 && hour <= 9) || (hour >= 17 && hour <= 19))
                baseMultiplier += 0.10m;

            // Weekend premium
            if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
                baseMultiplier += 0.05m;

            return baseMultiplier;
        }

        public async Task<List<decimal>> GetCompetitorPricesAsync(Load load)
        {
            // This would fetch actual competitor prices from external APIs
            // For now, generating sample competitor prices based on our calculation
            var basePrice = await GetBasePriceAsync(load);

            return new List<decimal>
            {
                basePrice * 0.95m, // 5% lower
                basePrice * 1.05m, // 5% higher  
                basePrice * 0.90m, // 10% lower
                basePrice * 1.15m, // 15% higher
                basePrice * 1.02m  // 2% higher
            };
        }

        public async Task<decimal> GetAverageMarketPriceAsync(LoadType loadType, decimal distanceKm, decimal weight)
        {
            // Would query historical market data from database
            var basePrice = distanceKm * BasePricePerKm + weight * BasePricePerKg;

            // Load type multipliers
            var multiplier = loadType switch
            {
                LoadType.Hazardous => 1.4m,
                LoadType.Refrigerated => 1.3m,
                LoadType.Oversized => 1.5m,
                LoadType.Fragile => 1.2m,
                LoadType.GeneralCargo => 1.0m,
                _ => 1.1m
            };

            return basePrice * multiplier;
        }

        public async Task<bool> AcceptCalculationAsync(long calculationId, decimal agreedPrice, string acceptedBy)
        {
            await _pricingRepository.MarkAsAcceptedAsync(calculationId, agreedPrice, acceptedBy);
            return true;
        }

        public async Task<PricingCalculation?> GetLatestCalculationAsync(long loadId)
        {
            return await _pricingRepository.GetLatestCalculationForLoadAsync(loadId);
        }

        public async Task<List<PricingCalculation>> GetCalculationHistoryAsync(long loadId)
        {
            return await _pricingRepository.GetCalculationHistoryAsync(loadId);
        }

        public async Task<decimal> GetAcceptanceRateAsync(string? algorithmVersion = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _pricingRepository.GetAcceptanceRateAsync(algorithmVersion, fromDate, toDate);
        }

        public async Task<decimal> GetAveragePriceVarianceAsync(string? algorithmVersion = null)
        {
            return await _pricingRepository.GetAveragePriceVarianceAsync(algorithmVersion);
        }

        public async Task<PricingCalculation> AdjustPriceManuallyAsync(long calculationId, decimal adjustment, string reason, string adjustedBy)
        {
            var calculation = await _pricingRepository.GetByIdAsync(calculationId);
            if (calculation == null)
                throw new ArgumentException("Calculation not found");

            calculation.IsManuallyAdjusted = true;
            calculation.ManualAdjustment = adjustment;
            calculation.ManualAdjustmentReason = reason;
            calculation.CalculatedPrice += adjustment;
            calculation.UpdatedUser = adjustedBy;

            _pricingRepository.Update(calculation);
            await _unitOfWork.SaveChangesAsync();

            return calculation;
        }

        public async Task<Dictionary<string, decimal>> GetPricingFactorsAsync(Load load)
        {
            return new Dictionary<string, decimal>
            {
                ["DistanceFactor"] = CalculateDistanceFactor(load.DistanceKm ?? 0),
                ["WeightFactor"] = CalculateWeightFactor(load.Weight),
                ["VolumeFactor"] = CalculateVolumeFactor(load.Volume),
                ["UrgencyFactor"] = CalculateUrgencyFactor(load.PickupDateTime, load.DeliveryDeadline),
                ["SeasonalFactor"] = CalculateSeasonalFactor(load.PickupDateTime),
                ["SpecialRequirementsFactor"] = CalculateSpecialRequirementsFactor(load.SpecialRequirements),
                ["TimeOfDayFactor"] = CalculateTimeOfDayFactor(load.PickupDateTime),
                ["DayOfWeekFactor"] = CalculateDayOfWeekFactor(load.PickupDateTime)
            };
        }

        public async Task UpdateAlgorithmParametersAsync(string algorithmVersion, Dictionary<string, decimal> parameters)
        {
            // This would update algorithm parameters in a configuration store
            // For now, just a placeholder implementation
            await Task.CompletedTask;
        }

        #region Private Helper Methods

        private decimal CalculateDistanceFactor(decimal distanceKm)
        {
            // Longer distances get slight economy of scale
            if (distanceKm >= 1000) return 0.95m;
            if (distanceKm >= 500) return 0.97m;
            if (distanceKm >= 200) return 1.0m;
            if (distanceKm >= 50) return 1.05m;
            return 1.1m; // Short distances are more expensive per km
        }

        private decimal CalculateWeightFactor(decimal weight)
        {
            // Heavy loads require special handling
            if (weight >= 20000) return 1.3m; // Over 20 tons
            if (weight >= 10000) return 1.2m; // Over 10 tons
            if (weight >= 5000) return 1.1m;  // Over 5 tons
            return 1.0m;
        }

        private decimal CalculateVolumeFactor(decimal? volume)
        {
            if (!volume.HasValue) return 1.0m;

            // Large volume loads may require special vehicles
            if (volume >= 100) return 1.2m;
            if (volume >= 50) return 1.1m;
            return 1.0m;
        }

        private decimal CalculateUrgencyFactor(DateTime pickupDateTime, DateTime deliveryDeadline)
        {
            var timeWindow = deliveryDeadline - pickupDateTime;
            var hoursAvailable = timeWindow.TotalHours;

            // Tight delivery windows increase price
            if (hoursAvailable <= 4) return 1.5m;   // Same day delivery
            if (hoursAvailable <= 12) return 1.3m;  // Next day delivery
            if (hoursAvailable <= 24) return 1.2m;  // 24 hour delivery
            if (hoursAvailable <= 48) return 1.1m;  // 2 day delivery
            return 1.0m; // Standard delivery
        }

        private decimal CalculateSeasonalFactor(DateTime pickupDateTime)
        {
            var month = pickupDateTime.Month;

            // Higher demand during certain months
            if (month == 12 || month == 1) return 1.15m; // Holiday season
            if (month >= 6 && month <= 8) return 1.1m;   // Summer season
            if (month == 11) return 1.05m;               // Pre-holiday
            return 1.0m;
        }

        private decimal CalculateSpecialRequirementsFactor(List<SpecialRequirement> requirements)
        {
            if (requirements == null || !requirements.Any())
                return 1.0m;

            decimal factor = 1.0m;
            foreach (var requirement in requirements)
            {
                factor += requirement switch
                {
                    SpecialRequirement.Hazardous => 0.4m,
                    SpecialRequirement.Refrigerated => 0.3m,
                    SpecialRequirement.Oversized => 0.5m,
                    SpecialRequirement.HighValue => 0.2m,
                    SpecialRequirement.Fragile => 0.15m,
                    SpecialRequirement.ExpressDelivery => 0.3m,
                    _ => 0.1m
                };
            }

            return factor;
        }

        private decimal CalculateTimeOfDayFactor(DateTime pickupDateTime)
        {
            var hour = pickupDateTime.Hour;

            // Peak hours have higher rates
            if (hour >= 7 && hour <= 9) return 1.1m;   // Morning rush
            if (hour >= 17 && hour <= 19) return 1.1m; // Evening rush
            if (hour >= 22 || hour <= 5) return 1.2m;  // Night deliveries
            return 1.0m;
        }

        private decimal CalculateDayOfWeekFactor(DateTime pickupDateTime)
        {
            return pickupDateTime.DayOfWeek switch
            {
                DayOfWeek.Saturday => 1.1m,
                DayOfWeek.Sunday => 1.15m,
                DayOfWeek.Friday => 1.05m,
                _ => 1.0m
            };
        }

        private decimal CalculateDriverCost(decimal distanceKm)
        {
            // Daily driver wage + per km allowance
            decimal dailyWage = 300m;
            decimal perKmAllowance = 0.5m;

            // Assume 8-hour workday at 80km/h average = 640km max per day
            var days = Math.Ceiling(distanceKm / 640);

            return (dailyWage * (decimal)days) + (perKmAllowance * distanceKm);
        }

        private decimal CalculateVehicleCost(LoadType loadType)
        {
            // Vehicle depreciation and maintenance per day
            return loadType switch
            {
                LoadType.Hazardous => 150m,
                LoadType.Refrigerated => 120m,
                LoadType.Oversized => 200m,
                _ => 80m
            };
        }

        private string GetMarketDemandLevel(decimal demandFactor)
        {
            if (demandFactor >= 1.2m) return "HIGH";
            if (demandFactor <= 0.9m) return "LOW";
            return "MEDIUM";
        }

        private async Task<int> GetAvailableDriversCount(Load load)
        {
            // This would query the driver repository
            // For now, return a simulated count
            return new Random().Next(5, 50);
        }

        private bool IsHighDemandRoute(string pickupCity, string deliveryCity)
        {
            var highDemandCities = new[] { "Istanbul", "Ankara", "Izmir", "Bursa", "Antalya" };
            return highDemandCities.Contains(pickupCity) || highDemandCities.Contains(deliveryCity);
        }

        #endregion
    }
}