using AknaLoad.Api.DTOs.Pricing;
using AknaLoad.Application.Services;
using Microsoft.AspNetCore.Mvc;
using static AknaLoad.Application.Services.PricingService;

namespace AknaLoad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly PricingService _pricingService;
        private readonly ILogger<PricingController> _logger;

        public PricingController(PricingService pricingService, ILogger<PricingController> logger)
        {
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Calculate price for a load based on various factors
        /// </summary>
        /// <param name="request">Price calculation request parameters</param>
        /// <returns>Calculated price with breakdown of factors</returns>
        [HttpPost("calculate")]
        [ProducesResponseType(typeof(PriceCalculationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculatePrice([FromBody] PriceCalculationRequestDto request)
        {
            try
            {
                if (request.DistanceKm <= 0)
                {
                    return BadRequest(new { error = "Distance must be greater than 0" });
                }

                if (request.Weight <= 0)
                {
                    return BadRequest(new { error = "Weight must be greater than 0" });
                }

                _logger.LogInformation("Calculating price for {Distance}km, {Weight}kg",
                    request.DistanceKm, request.Weight);

                var resultObj = await _pricingService.CalculatePriceAsync(
                    request.DistanceKm,
                    request.Weight,
                    request.Volume,
                    request.LoadType,
                    request.SpecialRequirements,
                    request.PickupDateTime,
                    request.DeliveryDeadline,
                    request.UseAIOptimization);

                // Cast the result from object to PricingResult
                var result = resultObj as PricingResult
                    ?? throw new InvalidOperationException("Failed to cast pricing result");

                var response = new PriceCalculationResponseDto
                {
                    BasePrice = result.BasePrice,
                    FinalPrice = result.FinalPrice,
                    OptimizedPrice = result.OptimizedPrice,
                    RecommendedPrice = result.RecommendedPrice,
                    Factors = new PricingFactorsDto
                    {
                        DistanceFactor = result.Factors.DistanceFactor,
                        WeightFactor = result.Factors.WeightFactor,
                        VolumeFactor = result.Factors.VolumeFactor,
                        HazardousFactor = result.Factors.HazardousFactor,
                        RefrigeratedFactor = result.Factors.RefrigeratedFactor,
                        WeekendFactor = result.Factors.WeekendFactor,
                        PeakHoursFactor = result.Factors.PeakHoursFactor,
                        UrgencyFactor = result.Factors.UrgencyFactor,
                        SpecialRequirementsFactor = result.Factors.SpecialRequirementsFactor,
                        TotalMultiplier = result.Factors.TotalMultiplier
                    },
                    AIOptimizationDetails = result.AIOptimizationDetails,
                    CalculatedAt = result.CalculatedAt,
                    AlgorithmVersion = result.AlgorithmVersion
                };

                _logger.LogInformation("Price calculated: {Price} TL (Base: {BasePrice} TL, AI Optimized: {Optimized})",
                    response.RecommendedPrice, response.BasePrice, response.OptimizedPrice.HasValue);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while calculating price");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price");
                return StatusCode(500, new { error = "An error occurred while calculating the price" });
            }
        }

        /// <summary>
        /// Get vehicle type recommendations based on load characteristics
        /// </summary>
        /// <param name="request">Vehicle matching request parameters</param>
        /// <returns>List of recommended vehicle types with suitability scores</returns>
        [HttpPost("vehicle-match")]
        [ProducesResponseType(typeof(VehicleMatchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetVehicleMatch([FromBody] VehicleMatchRequestDto request)
        {
            try
            {
                if (request.Weight <= 0)
                {
                    return BadRequest(new { error = "Weight must be greater than 0" });
                }

                _logger.LogInformation("Getting vehicle matches for {Weight}kg, LoadType: {LoadType}",
                    request.Weight, request.LoadType);

                var matchesObj = await _pricingService.GetVehicleMatchesAsync(
                    request.Weight,
                    request.Volume,
                    request.LoadType,
                    request.SpecialRequirements,
                    request.DistanceKm,
                    request.Dimensions?.Length,
                    request.Dimensions?.Width,
                    request.Dimensions?.Height,
                    request.UseAIRecommendation);

                // Cast the result from object to List<VehicleMatch>
                var matches = matchesObj as List<VehicleMatch>
                    ?? throw new InvalidOperationException("Failed to cast vehicle matches result");

                var response = new VehicleMatchResponseDto
                {
                    RecommendedVehicles = matches.Select(m => new VehicleRecommendationDto
                    {
                        VehicleType = m.VehicleType,
                        SuitabilityScore = m.SuitabilityScore,
                        Reason = m.Reason,
                        Capacity = new VehicleCapacityDto
                        {
                            MaxWeight = m.MaxWeight,
                            MaxVolume = m.MaxVolume,
                            Length = m.Length,
                            Width = m.Width,
                            Height = m.Height
                        },
                        EstimatedCost = m.EstimatedCost
                    }).OrderByDescending(v => v.SuitabilityScore).ToList(),
                    AIRecommendationDetails = request.UseAIRecommendation
                        ? $"AI recommended {matches.Count} vehicle types based on load characteristics"
                        : "Rule-based matching used",
                    CalculatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Found {Count} vehicle matches, top match: {VehicleType} (score: {Score})",
                    response.RecommendedVehicles.Count,
                    response.RecommendedVehicles.FirstOrDefault()?.VehicleType,
                    response.RecommendedVehicles.FirstOrDefault()?.SuitabilityScore);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while matching vehicles");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error matching vehicles");
                return StatusCode(500, new { error = "An error occurred while matching vehicles" });
            }
        }

        /// <summary>
        /// Get pricing information and recommendations
        /// </summary>
        [HttpGet("info")]
        [ProducesResponseType(typeof(PricingInfoDto), StatusCodes.Status200OK)]
        public IActionResult GetPricingInfo()
        {
            var info = new PricingInfoDto
            {
                BaseRates = new BaseRatesDto
                {
                    DistanceRate = 3.5m,
                    WeightRate = 0.15m,
                    Currency = "TRL"
                },
                Factors = new List<PricingFactorInfoDto>
                {
                    new() { Name = "Hacim", Description = "Büyük hacimli yükler için %15 artış (>20m³)", Multiplier = "1.15x" },
                    new() { Name = "Tehlikeli Madde", Description = "Tehlikeli madde taşımacılığı için %50 artış", Multiplier = "1.50x" },
                    new() { Name = "Soğutmalı", Description = "Soğutmalı taşıma için %30 artış", Multiplier = "1.30x" },
                    new() { Name = "Hafta Sonu", Description = "Cumartesi/Pazar teslimi için %25 artış", Multiplier = "1.25x" },
                    new() { Name = "Peak Hours", Description = "Yoğun saatlerde (08-10, 17-19) %15 artış", Multiplier = "1.15x" },
                    new() { Name = "Acil", Description = "24 saatten az: %40, 48 saatten az: %20 artış", Multiplier = "1.20-1.40x" },
                    new() { Name = "Özel Gereksinimler", Description = "Her özel gereksinim için %5 artış", Multiplier = "1.05x+" }
                },
                AIOptimization = "Gemini AI ile piyasa koşulları ve talep analizi yapılarak fiyat optimize edilir"
            };

            return Ok(info);
        }
    }

    // Info DTOs
    public class PricingInfoDto
    {
        public BaseRatesDto BaseRates { get; set; } = new();
        public List<PricingFactorInfoDto> Factors { get; set; } = new();
        public string AIOptimization { get; set; } = string.Empty;
    }

    public class BaseRatesDto
    {
        public decimal DistanceRate { get; set; }
        public decimal WeightRate { get; set; }
        public string Currency { get; set; } = "TRL";
    }

    public class PricingFactorInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Multiplier { get; set; } = string.Empty;
    }
}
