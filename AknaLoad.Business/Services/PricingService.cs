using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Domain.Interfaces.Services;
using AknaLoad.Domain.Interfaces.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace AknaLoad.Application.Services
{
    public class PricingService : IPricingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPricingCalculationRepository _pricingRepository;
        private readonly IGeminiAIService _geminiAIService;
        private readonly ILogger<PricingService> _logger;

        // Pricing constants
        private const decimal DISTANCE_RATE = 3.5m;  // TL per km
        private const decimal WEIGHT_RATE = 0.15m;   // TL per kg
        private const string ALGORITHM_VERSION = "1.0";

        public PricingService(
            IUnitOfWork unitOfWork,
            IPricingCalculationRepository pricingRepository,
            IGeminiAIService geminiAIService,
            ILogger<PricingService> logger)
        {
            _unitOfWork = unitOfWork;
            _pricingRepository = pricingRepository;
            _geminiAIService = geminiAIService;
            _logger = logger;
        }

        public async Task<PricingResult> CalculatePriceAsync(
            decimal distanceKm,
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements,
            DateTime pickupDateTime,
            DateTime deliveryDeadline,
            bool useAIOptimization = true)
        {
            try
            {
                // Calculate base price: (distance × 3.5) + (weight × 0.15)
                var basePrice = (distanceKm * DISTANCE_RATE) + (weight * WEIGHT_RATE);

                _logger.LogInformation("Base price calculated: {BasePrice} TL (Distance: {Distance} km, Weight: {Weight} kg)",
                    basePrice, distanceKm, weight);

                // Calculate all pricing factors
                var factors = CalculatePricingFactors(
                    volume,
                    specialRequirements,
                    pickupDateTime,
                    deliveryDeadline,
                    loadType);

                // Apply all factors to base price
                var finalPrice = basePrice * factors.TotalMultiplier;

                _logger.LogInformation("Final price after factors: {FinalPrice} TL (Total multiplier: {Multiplier})",
                    finalPrice, factors.TotalMultiplier);

                // Get AI optimized price if requested
                decimal? optimizedPrice = null;
                string? aiDetails = null;

                if (useAIOptimization)
                {
                    try
                    {
                        optimizedPrice = await _geminiAIService.OptimizePricingAsync(
                            finalPrice,
                            distanceKm,
                            weight,
                            volume,
                            loadType,
                            specialRequirements,
                            pickupDateTime,
                            deliveryDeadline);

                        aiDetails = $"AI optimized from {finalPrice:F2} TL to {optimizedPrice:F2} TL";
                        _logger.LogInformation("AI optimization complete: {Details}", aiDetails);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "AI optimization failed, using calculated price");
                    }
                }

                var result = new PricingResult
                {
                    BasePrice = basePrice,
                    FinalPrice = finalPrice,
                    OptimizedPrice = optimizedPrice,
                    RecommendedPrice = optimizedPrice ?? finalPrice,
                    Factors = factors,
                    AIOptimizationDetails = aiDetails,
                    CalculatedAt = DateTime.UtcNow,
                    AlgorithmVersion = ALGORITHM_VERSION
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price");
                throw;
            }
        }

        public async Task<List<VehicleMatch>> GetVehicleMatchesAsync(
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements,
            decimal? distanceKm = null,
            decimal? length = null,
            decimal? width = null,
            decimal? height = null,
            bool useAIRecommendation = true)
        {
            try
            {
                List<VehicleRecommendation> aiRecommendations = new();

                if (useAIRecommendation)
                {
                    try
                    {
                        aiRecommendations = await _geminiAIService.GetVehicleRecommendationsAsync(
                            weight,
                            volume,
                            loadType,
                            specialRequirements,
                            length,
                            width,
                            height);

                        _logger.LogInformation("AI recommended {Count} vehicle types", aiRecommendations.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "AI vehicle recommendation failed, using rule-based matching");
                    }
                }

                // Convert AI recommendations to VehicleMatch
                var matches = aiRecommendations.Select(r => new VehicleMatch
                {
                    VehicleType = r.VehicleType,
                    SuitabilityScore = r.SuitabilityScore,
                    Reason = r.Reason,
                    MaxWeight = r.MaxWeight,
                    MaxVolume = r.MaxVolume,
                    Length = null, // AI doesn't provide these details
                    Width = null,
                    Height = null,
                    EstimatedCost = r.EstimatedCost,
                    CalculatedAt = DateTime.UtcNow
                }).ToList();

                if (!matches.Any())
                {
                    // Fallback to rule-based matching
                    matches = GetRuleBasedVehicleMatches(weight, volume, loadType, specialRequirements);
                    _logger.LogInformation("Using rule-based vehicle matching, found {Count} matches", matches.Count);
                }

                return matches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle matches");
                throw;
            }
        }

        public async Task<PricingCalculation> SavePricingCalculationAsync(
            long loadId,
            PricingResult pricingResult)
        {
            try
            {
                var calculation = new PricingCalculation
                {
                    LoadId = loadId,
                    AlgorithmVersion = pricingResult.AlgorithmVersion,
                    CalculatedPrice = pricingResult.RecommendedPrice,
                    BasePrice = pricingResult.BasePrice,
                    DistanceFactor = pricingResult.Factors.DistanceFactor,
                    WeightFactor = pricingResult.Factors.WeightFactor,
                    VolumeFactor = pricingResult.Factors.VolumeFactor,
                    UrgencyFactor = pricingResult.Factors.UrgencyFactor,
                    SeasonalFactor = pricingResult.Factors.WeekendFactor,
                    SpecialRequirementsFactor = pricingResult.Factors.SpecialRequirementsFactor,
                    TimeOfDayFactor = pricingResult.Factors.PeakHoursFactor,
                    DayOfWeekFactor = pricingResult.Factors.WeekendFactor,
                    CalculatedAt = pricingResult.CalculatedAt,
                    MarketDemand = "MEDIUM",
                    FuelCostEstimate = 0,
                    TollCostEstimate = 0,
                    DriverCostEstimate = 0,
                    VehicleCostEstimate = 0,
                    AverageMarketPrice = pricingResult.RecommendedPrice,
                    AvailableDriversCount = 0
                };

                await _pricingRepository.AddAsync(calculation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Pricing calculation saved for load {LoadId}", loadId);
                return calculation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving pricing calculation for load {LoadId}", loadId);
                throw;
            }
        }

        private PricingFactors CalculatePricingFactors(
            decimal? volume,
            List<SpecialRequirement> specialRequirements,
            DateTime pickupDateTime,
            DateTime deliveryDeadline,
            LoadType loadType)
        {
            var factors = new PricingFactors
            {
                DistanceFactor = 1.0m,
                WeightFactor = 1.0m,
                VolumeFactor = 1.0m,
                HazardousFactor = 1.0m,
                RefrigeratedFactor = 1.0m,
                WeekendFactor = 1.0m,
                PeakHoursFactor = 1.0m,
                UrgencyFactor = 1.0m,
                SpecialRequirementsFactor = 1.0m
            };

            // Volume factor (if large volume)
            if (volume.HasValue && volume.Value > 20)
            {
                factors.VolumeFactor = 1.15m; // 15% increase for large volumes
            }

            // Hazardous material factor
            if (specialRequirements.Contains(SpecialRequirement.Hazardous) ||
                specialRequirements.Contains(SpecialRequirement.FlammableLiquid) ||
                specialRequirements.Contains(SpecialRequirement.CorrosiveMaterial))
            {
                factors.HazardousFactor = 1.5m; // 50% increase
            }

            // Refrigerated/Cold chain factor
            if (specialRequirements.Contains(SpecialRequirement.Refrigerated) ||
                specialRequirements.Contains(SpecialRequirement.ColdChain) ||
                specialRequirements.Contains(SpecialRequirement.TemperatureControlled))
            {
                factors.RefrigeratedFactor = 1.3m; // 30% increase
            }

            // Weekend factor
            if (pickupDateTime.DayOfWeek == DayOfWeek.Saturday ||
                pickupDateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                factors.WeekendFactor = 1.25m; // 25% increase
            }

            // Peak hours factor (8-10 AM or 5-7 PM)
            var hour = pickupDateTime.Hour;
            if ((hour >= 8 && hour < 10) || (hour >= 17 && hour < 19))
            {
                factors.PeakHoursFactor = 1.15m; // 15% increase
            }

            // Urgency factor (less than 24 hours notice)
            var hoursUntilPickup = (pickupDateTime - DateTime.UtcNow).TotalHours;
            if (hoursUntilPickup < 24)
            {
                factors.UrgencyFactor = 1.4m; // 40% increase for urgent deliveries
            }
            else if (hoursUntilPickup < 48)
            {
                factors.UrgencyFactor = 1.2m; // 20% increase
            }

            // Other special requirements factor
            var otherRequirements = specialRequirements
                .Where(r => r != SpecialRequirement.None &&
                           r != SpecialRequirement.Hazardous &&
                           r != SpecialRequirement.Refrigerated &&
                           r != SpecialRequirement.ColdChain &&
                           r != SpecialRequirement.TemperatureControlled &&
                           r != SpecialRequirement.FlammableLiquid &&
                           r != SpecialRequirement.CorrosiveMaterial)
                .ToList();

            if (otherRequirements.Any())
            {
                // Each additional requirement adds 5%
                factors.SpecialRequirementsFactor = 1.0m + (otherRequirements.Count * 0.05m);
            }

            // Calculate total multiplier
            factors.TotalMultiplier =
                factors.DistanceFactor *
                factors.WeightFactor *
                factors.VolumeFactor *
                factors.HazardousFactor *
                factors.RefrigeratedFactor *
                factors.WeekendFactor *
                factors.PeakHoursFactor *
                factors.UrgencyFactor *
                factors.SpecialRequirementsFactor;

            return factors;
        }

        private List<VehicleMatch> GetRuleBasedVehicleMatches(
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements)
        {
            var matches = new List<VehicleMatch>();

            var isRefrigerated = specialRequirements.Contains(SpecialRequirement.Refrigerated) ||
                                specialRequirements.Contains(SpecialRequirement.ColdChain);
            var isHazardous = specialRequirements.Contains(SpecialRequirement.Hazardous);
            var isOversized = specialRequirements.Contains(SpecialRequirement.Oversized);

            if (isHazardous)
            {
                matches.Add(new VehicleMatch
                {
                    VehicleType = "ADR Sertifikalı Tır",
                    SuitabilityScore = 95,
                    Reason = "Tehlikeli madde taşımacılığı için ADR sertifikası gereklidir",
                    MaxWeight = 24000,
                    MaxVolume = 90,
                    EstimatedCost = weight * 5.0m
                });
            }
            else if (isOversized)
            {
                matches.Add(new VehicleMatch
                {
                    VehicleType = "Açık Kasa Tır",
                    SuitabilityScore = 90,
                    Reason = "Gabaritli yükler için açık kasa tercih edilir",
                    MaxWeight = 24000,
                    EstimatedCost = weight * 4.5m
                });
            }
            else if (weight <= 1000)
            {
                matches.Add(new VehicleMatch
                {
                    VehicleType = isRefrigerated ? "Frigorifik Van" : "Van/Panelvan",
                    SuitabilityScore = 90,
                    Reason = isRefrigerated ? "Hafif yükler için soğutmalı van" : "Hafif yükler için ekonomik seçenek",
                    MaxWeight = 1000,
                    MaxVolume = 7,
                    EstimatedCost = weight * (isRefrigerated ? 3.5m : 2.0m)
                });
            }
            else if (weight <= 3500)
            {
                matches.Add(new VehicleMatch
                {
                    VehicleType = isRefrigerated ? "Frigorifik Kamyonet" : "Kamyonet",
                    SuitabilityScore = 90,
                    Reason = isRefrigerated ? "Orta ağırlıktaki yükler için soğutmalı kamyonet" : "Orta ağırlıktaki yükler için ideal",
                    MaxWeight = 3500,
                    MaxVolume = 15,
                    EstimatedCost = weight * (isRefrigerated ? 3.0m : 2.5m)
                });
            }
            else
            {
                matches.Add(new VehicleMatch
                {
                    VehicleType = isRefrigerated ? "Frigorifik Tır" : "Tır",
                    SuitabilityScore = 90,
                    Reason = isRefrigerated ? "Ağır yükler için soğutmalı tır" : "Ağır yükler için standart tır",
                    MaxWeight = 24000,
                    MaxVolume = 90,
                    EstimatedCost = weight * (isRefrigerated ? 4.0m : 3.5m)
                });
            }

            foreach (var match in matches)
            {
                match.CalculatedAt = DateTime.UtcNow;
            }

            return matches;
        }
    }

    // Service DTOs
    public class PricingResult
    {
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal? OptimizedPrice { get; set; }
        public decimal RecommendedPrice { get; set; }
        public PricingFactors Factors { get; set; } = new();
        public string? AIOptimizationDetails { get; set; }
        public DateTime CalculatedAt { get; set; }
        public string AlgorithmVersion { get; set; } = "1.0";
    }

    public class PricingFactors
    {
        public decimal DistanceFactor { get; set; } = 1.0m;
        public decimal WeightFactor { get; set; } = 1.0m;
        public decimal VolumeFactor { get; set; } = 1.0m;
        public decimal HazardousFactor { get; set; } = 1.0m;
        public decimal RefrigeratedFactor { get; set; } = 1.0m;
        public decimal WeekendFactor { get; set; } = 1.0m;
        public decimal PeakHoursFactor { get; set; } = 1.0m;
        public decimal UrgencyFactor { get; set; } = 1.0m;
        public decimal SpecialRequirementsFactor { get; set; } = 1.0m;
        public decimal TotalMultiplier { get; set; } = 1.0m;
    }

    public class VehicleMatch
    {
        public string VehicleType { get; set; } = string.Empty;
        public int SuitabilityScore { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal MaxWeight { get; set; }
        public decimal? MaxVolume { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? EstimatedCost { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}