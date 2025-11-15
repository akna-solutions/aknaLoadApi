namespace AknaLoad.Api.DTOs.Pricing
{
    /// <summary>
    /// Response DTO for price calculation
    /// </summary>
    public class PriceCalculationResponseDto
    {
        /// <summary>
        /// Calculated base price
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Final calculated price (after all factors)
        /// </summary>
        public decimal FinalPrice { get; set; }

        /// <summary>
        /// Price with AI optimization (if requested)
        /// </summary>
        public decimal? OptimizedPrice { get; set; }

        /// <summary>
        /// Recommended price to use
        /// </summary>
        public decimal RecommendedPrice { get; set; }

        /// <summary>
        /// Pricing factors applied
        /// </summary>
        public PricingFactorsDto Factors { get; set; } = new();

        /// <summary>
        /// AI optimization details (if used)
        /// </summary>
        public string? AIOptimizationDetails { get; set; }

        /// <summary>
        /// Calculation timestamp
        /// </summary>
        public DateTime CalculatedAt { get; set; }

        /// <summary>
        /// Algorithm version used
        /// </summary>
        public string AlgorithmVersion { get; set; } = "1.0";
    }

    /// <summary>
    /// Pricing factors breakdown
    /// </summary>
    public class PricingFactorsDto
    {
        /// <summary>
        /// Distance factor multiplier
        /// </summary>
        public decimal DistanceFactor { get; set; } = 1.0m;

        /// <summary>
        /// Weight factor multiplier
        /// </summary>
        public decimal WeightFactor { get; set; } = 1.0m;

        /// <summary>
        /// Volume factor multiplier
        /// </summary>
        public decimal VolumeFactor { get; set; } = 1.0m;

        /// <summary>
        /// Hazardous material multiplier
        /// </summary>
        public decimal HazardousFactor { get; set; } = 1.0m;

        /// <summary>
        /// Refrigerated/cold chain multiplier
        /// </summary>
        public decimal RefrigeratedFactor { get; set; } = 1.0m;

        /// <summary>
        /// Weekend delivery multiplier
        /// </summary>
        public decimal WeekendFactor { get; set; } = 1.0m;

        /// <summary>
        /// Peak hours multiplier
        /// </summary>
        public decimal PeakHoursFactor { get; set; } = 1.0m;

        /// <summary>
        /// Urgency factor (time sensitivity)
        /// </summary>
        public decimal UrgencyFactor { get; set; } = 1.0m;

        /// <summary>
        /// Special requirements factor
        /// </summary>
        public decimal SpecialRequirementsFactor { get; set; } = 1.0m;

        /// <summary>
        /// Total combined multiplier
        /// </summary>
        public decimal TotalMultiplier { get; set; } = 1.0m;
    }
}
