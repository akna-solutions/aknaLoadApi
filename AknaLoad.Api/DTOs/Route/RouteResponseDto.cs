using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Route
{
    /// <summary>
    /// Route response DTO
    /// </summary>
    public class RouteResponseDto
    {
        public long Id { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public long? LoadId { get; set; }

        // Route Details
        public decimal TotalDistance { get; set; }
        public int EstimatedDuration { get; set; }
        public decimal? TollCost { get; set; }
        public decimal? FuelCost { get; set; }
        public RouteType RouteType { get; set; }

        // Traffic and Conditions
        public string TrafficLevel { get; set; } = "MEDIUM";
        public string? RoadConditions { get; set; }
        public string? WeatherConditions { get; set; }
        public decimal DifficultyScore { get; set; }

        // Optimization Data
        public bool IsOptimized { get; set; }

        // Time Windows
        public DateTime? EarliestDepartureTime { get; set; }
        public DateTime? LatestArrivalTime { get; set; }

        // Performance Data
        public int UsageCount { get; set; }
        public decimal AverageActualDuration { get; set; }
        public decimal AverageActualDistance { get; set; }
        public DateTime? LastUsedAt { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
