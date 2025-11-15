using AknaLoad.Api.DTOs.Route;
using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Load
{
    /// <summary>
    /// Full load response DTO
    /// </summary>
    public class LoadResponseDto
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LoadStatus Status { get; set; }
        public string LoadCode { get; set; } = string.Empty;

        // Multi-Stop Configuration
        public bool IsMultiStop { get; set; }
        public LoadRoutingStrategy RoutingStrategy { get; set; }
        public int TotalStops { get; set; }
        public List<LoadStopResponseDto> LoadStops { get; set; } = new();

        // Load Properties
        public decimal Weight { get; set; }
        public decimal? Volume { get; set; }
        public DimensionsDto? Dimensions { get; set; }
        public LoadType LoadType { get; set; }
        public List<SpecialRequirement> SpecialRequirements { get; set; } = new();

        // Route Information
        public decimal? TotalDistanceKm { get; set; }
        public int? EstimatedTotalDurationMinutes { get; set; }
        public DateTime? EarliestPickupTime { get; set; }
        public DateTime? LatestDeliveryTime { get; set; }

        // Pricing
        public decimal? FixedPrice { get; set; }

        // Contact
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Route Information (detailed)
        public RouteResponseDto? Route { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? MatchedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? MatchedDriverId { get; set; }
        public long? MatchedVehicleId { get; set; }
    }
}
