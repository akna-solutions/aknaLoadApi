using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Pricing
{
    /// <summary>
    /// Request DTO for price calculation
    /// </summary>
    public class PriceCalculationRequestDto
    {
        /// <summary>
        /// Distance in kilometers
        /// </summary>
        public decimal DistanceKm { get; set; }

        /// <summary>
        /// Weight in kilograms
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Volume in cubic meters (optional)
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Load type
        /// </summary>
        public LoadType LoadType { get; set; } = LoadType.GeneralCargo;

        /// <summary>
        /// Special requirements list
        /// </summary>
        public List<SpecialRequirement> SpecialRequirements { get; set; } = new();

        /// <summary>
        /// Pickup date and time
        /// </summary>
        public DateTime PickupDateTime { get; set; }

        /// <summary>
        /// Delivery deadline
        /// </summary>
        public DateTime DeliveryDeadline { get; set; }

        /// <summary>
        /// Origin city
        /// </summary>
        public string? OriginCity { get; set; }

        /// <summary>
        /// Destination city
        /// </summary>
        public string? DestinationCity { get; set; }

        /// <summary>
        /// Use AI optimization (default: true)
        /// </summary>
        public bool UseAIOptimization { get; set; } = true;
    }
}
