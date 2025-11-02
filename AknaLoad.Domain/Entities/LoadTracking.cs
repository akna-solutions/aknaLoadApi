using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("LoadTrackings")]
    public class LoadTracking : BaseEntity
    {
        public long LoadId { get; set; }
        public long DriverId { get; set; }
        public long MatchId { get; set; }

        // Current Status and Location
        public TrackingStatus Status { get; set; } = TrackingStatus.WaitingForPickup;
        public string CurrentLocationJson { get; set; } = string.Empty; // JSON serialized Location
        public decimal? Speed { get; set; } // km/h
        public int? Heading { get; set; } // degrees, 0-360

        // Timing Information
        public DateTime? EstimatedPickupTime { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public DateTime? ActualPickupTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        public DateTime Timestamp { get; set; }

        // Progress Information
        public decimal? DistanceRemaining { get; set; } // km
        public int? TimeRemaining { get; set; } // minutes
        public decimal? ProgressPercentage { get; set; } // 0-100

        // Documentation and Proof
        public string? Notes { get; set; }
        public string? PhotoUrlsJson { get; set; } // JSON array of photo URLs
        public string? DocumentUrlsJson { get; set; } // JSON array of document URLs
        public string? DigitalSignature { get; set; } // Base64 encoded signature
        public string? RecipientName { get; set; }
        public string? RecipientIdNumber { get; set; }

        // Exception Handling
        public bool HasException { get; set; } = false;
        public string? ExceptionType { get; set; } // DELAY, BREAKDOWN, ACCIDENT, WEATHER, etc.
        public string? ExceptionDescription { get; set; }
        public DateTime? ExceptionReportedAt { get; set; }
        public DateTime? ExceptionResolvedAt { get; set; }

        // Communication
        public string? DriverMessage { get; set; }
        public string? CustomerMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }

        // Vehicle Information at time of tracking
        public string? VehiclePlate { get; set; }
        public decimal? FuelLevel { get; set; } // percentage
        public int? EngineHours { get; set; }
        public decimal? Odometer { get; set; } // km

        // Quality Metrics
        public bool IsOnTime { get; set; } = true;
        public int? DelayMinutes { get; set; }
        public bool IsOnRoute { get; set; } = true;
        public decimal? RouteDeviation { get; set; } // km off planned route

        // Navigation Properties (not mapped to database)
        [NotMapped]
        public Location? CurrentLocation
        {
            get => string.IsNullOrEmpty(CurrentLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(CurrentLocationJson);
            set => CurrentLocationJson = value == null ? string.Empty :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> PhotoUrls
        {
            get => string.IsNullOrEmpty(PhotoUrlsJson) ? new List<string>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<string>>(PhotoUrlsJson) ?? new List<string>();
            set => PhotoUrlsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> DocumentUrls
        {
            get => string.IsNullOrEmpty(DocumentUrlsJson) ? new List<string>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<string>>(DocumentUrlsJson) ?? new List<string>();
            set => DocumentUrlsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        // Navigation Properties
        public virtual Load Load { get; set; } = null!;
        public virtual Driver Driver { get; set; } = null!;
        public virtual Match Match { get; set; } = null!;
    }
}