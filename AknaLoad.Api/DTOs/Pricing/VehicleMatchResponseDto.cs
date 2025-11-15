namespace AknaLoad.Api.DTOs.Pricing
{
    /// <summary>
    /// Response DTO for vehicle type matching
    /// </summary>
    public class VehicleMatchResponseDto
    {
        /// <summary>
        /// Recommended vehicle types (ordered by suitability)
        /// </summary>
        public List<VehicleRecommendationDto> RecommendedVehicles { get; set; } = new();

        /// <summary>
        /// AI recommendation details (if used)
        /// </summary>
        public string? AIRecommendationDetails { get; set; }

        /// <summary>
        /// Calculation timestamp
        /// </summary>
        public DateTime CalculatedAt { get; set; }
    }

    /// <summary>
    /// Vehicle recommendation details
    /// </summary>
    public class VehicleRecommendationDto
    {
        /// <summary>
        /// Vehicle type name
        /// </summary>
        public string VehicleType { get; set; } = string.Empty;

        /// <summary>
        /// Suitability score (0-100)
        /// </summary>
        public int SuitabilityScore { get; set; }

        /// <summary>
        /// Reason for recommendation
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Vehicle capacity information
        /// </summary>
        public VehicleCapacityDto? Capacity { get; set; }

        /// <summary>
        /// Estimated cost for this vehicle type
        /// </summary>
        public decimal? EstimatedCost { get; set; }
    }

    /// <summary>
    /// Vehicle capacity information
    /// </summary>
    public class VehicleCapacityDto
    {
        /// <summary>
        /// Max weight capacity in kg
        /// </summary>
        public decimal MaxWeight { get; set; }

        /// <summary>
        /// Max volume capacity in m³
        /// </summary>
        public decimal? MaxVolume { get; set; }

        /// <summary>
        /// Cargo area length in meters
        /// </summary>
        public decimal? Length { get; set; }

        /// <summary>
        /// Cargo area width in meters
        /// </summary>
        public decimal? Width { get; set; }

        /// <summary>
        /// Cargo area height in meters
        /// </summary>
        public decimal? Height { get; set; }
    }
}
