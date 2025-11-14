using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Load
{
    /// <summary>
    /// Load stop response DTO
    /// </summary>
    public class LoadStopResponseDto
    {
        public long Id { get; set; }
        public long LoadId { get; set; }
        public int StopOrder { get; set; }
        public LoadStopType StopType { get; set; }
        public LocationDto Location { get; set; } = null!;

        // Time Management
        public DateTime? EarliestTime { get; set; }
        public DateTime? LatestTime { get; set; }
        public DateTime? PlannedTime { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        // Load Quantities
        public decimal? PickupWeight { get; set; }
        public decimal? DeliveryWeight { get; set; }
        public decimal? PickupVolume { get; set; }
        public decimal? DeliveryVolume { get; set; }

        // Details
        public string? LoadDescription { get; set; }
        public string? SpecialInstructions { get; set; }
        public List<SpecialRequirement> SpecialRequirements { get; set; } = new();

        // Contact
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Status
        public LoadStopStatus Status { get; set; }
        public DateTime? ActualArrivalTime { get; set; }
        public DateTime? ActualDepartureTime { get; set; }
        public string? CompletionNotes { get; set; }
    }
}
