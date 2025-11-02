using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("Matches")]
    public class Match : BaseEntity
    {
        public long LoadId { get; set; }
        public long DriverId { get; set; }
        public long VehicleId { get; set; }
        public string MatchCode { get; set; } = string.Empty; // Auto-generated unique code

        // Matching Algorithm Results
        public decimal MatchScore { get; set; } // 0-100
        public string MatchingFactorsJson { get; set; } = string.Empty; // JSON of matching factors

        // Status ve Timing
        public MatchStatus Status { get; set; } = MatchStatus.Proposed;
        public DateTime ProposedAt { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? RejectionReason { get; set; }

        // Route Information
        public DateTime? EstimatedPickupTime { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
        public decimal? RouteDistance { get; set; }
        public int? RouteDurationMinutes { get; set; }
        public string? RouteDetailsJson { get; set; } // JSON of route waypoints

        // Actual Performance (filled after completion)
        public DateTime? ActualPickupTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        public decimal? ActualDistance { get; set; }
        public int? ActualDurationMinutes { get; set; }

        // Financial Details
        public decimal? AgreedPrice { get; set; }
        public decimal? DriverCommission { get; set; }
        public decimal? PlatformFee { get; set; }

        // Rating and Feedback (after completion)
        public decimal? LoadOwnerRating { get; set; } // 1-5
        public string? LoadOwnerFeedback { get; set; }
        public decimal? DriverRating { get; set; } // 1-5
        public string? DriverFeedback { get; set; }

        // Emergency and Support
        public bool HasEmergency { get; set; } = false;
        public string? EmergencyDescription { get; set; }
        public DateTime? EmergencyReportedAt { get; set; }

        // Navigation Properties
        public virtual Load Load { get; set; } = null!;
        public virtual Driver Driver { get; set; } = null!;
    }
}