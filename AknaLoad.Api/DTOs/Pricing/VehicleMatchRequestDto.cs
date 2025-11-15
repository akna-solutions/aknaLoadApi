using AknaLoad.Api.DTOs.Load;
using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Pricing
{
    /// <summary>
    /// Request DTO for vehicle type matching
    /// </summary>
    public class VehicleMatchRequestDto
    {
        /// <summary>
        /// Weight in kilograms
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Volume in cubic meters (optional)
        /// </summary>
        public decimal? Volume { get; set; }

        /// <summary>
        /// Dimensions (optional)
        /// </summary>
        public DimensionsDto? Dimensions { get; set; }

        /// <summary>
        /// Load type
        /// </summary>
        public LoadType LoadType { get; set; } = LoadType.GeneralCargo;

        /// <summary>
        /// Special requirements list
        /// </summary>
        public List<SpecialRequirement> SpecialRequirements { get; set; } = new();

        /// <summary>
        /// Distance in kilometers
        /// </summary>
        public decimal? DistanceKm { get; set; }

        /// <summary>
        /// Number of stops (for multi-stop loads)
        /// </summary>
        public int? NumberOfStops { get; set; }

        /// <summary>
        /// Use AI recommendation (default: true)
        /// </summary>
        public bool UseAIRecommendation { get; set; } = true;
    }
}
