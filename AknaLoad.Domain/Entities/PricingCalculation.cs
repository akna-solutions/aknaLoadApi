using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("PricingCalculations")]
    public class PricingCalculation : BaseEntity
    {
        public long LoadId { get; set; }
        public string AlgorithmVersion { get; set; } = string.Empty;
        public decimal CalculatedPrice { get; set; }

        // Base Pricing Components
        public decimal BasePrice { get; set; }
        public decimal DistanceFactor { get; set; }
        public decimal WeightFactor { get; set; }
        public decimal VolumeFactor { get; set; }
        public decimal UrgencyFactor { get; set; }
        public decimal DemandFactor { get; set; }
        public decimal SeasonalFactor { get; set; }
        public decimal SpecialRequirementsFactor { get; set; }

        // Cost Components
        public decimal FuelCostEstimate { get; set; }
        public decimal TollCostEstimate { get; set; }
        public decimal DriverCostEstimate { get; set; }
        public decimal VehicleCostEstimate { get; set; }

        // Market Data at calculation time
        public string? CompetitorPricesJson { get; set; } // JSON array of competitor prices
        public string MarketDemand { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH
        public int AvailableDriversCount { get; set; }
        public decimal AverageMarketPrice { get; set; }

        // Time and Location Factors
        public decimal TimeOfDayFactor { get; set; } = 1.0m;
        public decimal DayOfWeekFactor { get; set; } = 1.0m;
        public decimal RouteDifficultyFactor { get; set; } = 1.0m;
        public decimal ReturnLoadProbability { get; set; } = 0.5m;

        // Calculation Metadata
        public DateTime CalculatedAt { get; set; }
        public string CalculationInputsJson { get; set; } = string.Empty; // JSON of all inputs used
        public bool IsManuallyAdjusted { get; set; } = false;
        public decimal? ManualAdjustment { get; set; }
        public string? ManualAdjustmentReason { get; set; }

        // Performance Tracking
        public bool WasAccepted { get; set; } = false;
        public DateTime? AcceptedAt { get; set; }
        public decimal? FinalAgreedPrice { get; set; }
        public decimal? PriceVariancePercentage { get; set; }

        // Navigation Properties
        public virtual Load Load { get; set; } = null!;
    }
}