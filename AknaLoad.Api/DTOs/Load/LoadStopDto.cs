using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Load
{
    /// <summary>
    /// Load stop DTO for creating/updating stops
    /// </summary>
    public class LoadStopDto
    {
        public int StopOrder { get; set; }
        public LoadStopType StopType { get; set; }
        public LocationDto Location { get; set; } = null!;

        // Time Management
        public DateTime? EarliestTime { get; set; }
        public DateTime? LatestTime { get; set; }
        public DateTime? PlannedTime { get; set; }
        public int EstimatedDurationMinutes { get; set; } = 30;

        // Load Quantities
        public decimal? PickupWeight { get; set; }
        public decimal? DeliveryWeight { get; set; }
        public decimal? PickupVolume { get; set; }
        public decimal? DeliveryVolume { get; set; }

        // Details
        public string? LoadDescription { get; set; }
        public string? SpecialInstructions { get; set; }
        public List<SpecialRequirement>? SpecialRequirements { get; set; }

        // Contact
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }
}
