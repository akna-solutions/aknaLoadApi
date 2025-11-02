using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("Routes")]
    public class Route : BaseEntity
    {
        public string RouteCode { get; set; } = string.Empty; // Auto-generated unique code
        public string StartLocationJson { get; set; } = string.Empty; // JSON serialized Location
        public string EndLocationJson { get; set; } = string.Empty; // JSON serialized Location
        public string? WaypointsJson { get; set; } // JSON array of Location objects

        // Route Details
        public decimal TotalDistance { get; set; } // in kilometers
        public int EstimatedDuration { get; set; } // in minutes
        public decimal? TollCost { get; set; }
        public decimal? FuelCost { get; set; }
        public RouteType RouteType { get; set; } = RouteType.Optimal;

        // Traffic and Conditions
        public string TrafficLevel { get; set; } = "MEDIUM"; // LOW, MEDIUM, HIGH
        public string? RoadConditions { get; set; }
        public string? WeatherConditions { get; set; }
        public decimal DifficultyScore { get; set; } = 1.0m; // 0.5 (easy) to 2.0 (very difficult)

        // Optimization Data
        public bool IsOptimized { get; set; } = false;
        public string? AlternativeRoutesJson { get; set; } // JSON array of alternative route IDs
        public string? OptimizationParametersJson { get; set; } // JSON of optimization settings used

        // Route Geometry and Details
        public string? EncodedPolyline { get; set; } // Google polyline encoding
        public string? TurnByTurnInstructionsJson { get; set; } // JSON array of navigation instructions

        // Vehicle Restrictions
        public decimal? MaxVehicleHeight { get; set; } // in meters
        public decimal? MaxVehicleWidth { get; set; } // in meters
        public decimal? MaxVehicleWeight { get; set; } // in tons
        public bool HasTruckRestrictions { get; set; } = false;
        public bool HasHazmatRestrictions { get; set; } = false;

        // Time Windows
        public DateTime? EarliestDepartureTime { get; set; }
        public DateTime? LatestArrivalTime { get; set; }

        // Performance Data
        public int UsageCount { get; set; } = 0;
        public decimal AverageActualDuration { get; set; } = 0;
        public decimal AverageActualDistance { get; set; } = 0;
        public DateTime? LastUsedAt { get; set; }

        // Navigation Properties (not mapped to database)
        [NotMapped]
        public Location? StartLocation
        {
            get => string.IsNullOrEmpty(StartLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(StartLocationJson);
            set => StartLocationJson = value == null ? string.Empty :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Location? EndLocation
        {
            get => string.IsNullOrEmpty(EndLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(EndLocationJson);
            set => EndLocationJson = value == null ? string.Empty :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<Location> Waypoints
        {
            get => string.IsNullOrEmpty(WaypointsJson) ? new List<Location>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<Location>>(WaypointsJson) ?? new List<Location>();
            set => WaypointsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}