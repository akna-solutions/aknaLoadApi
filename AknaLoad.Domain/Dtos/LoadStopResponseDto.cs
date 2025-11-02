using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AknaLoad.Domain.Dtos
{
    /// <summary>
    /// Response DTO for load stops with status information
    /// </summary>
    public class LoadStopResponseDto
    {
        public long Id { get; set; }
        public int StopOrder { get; set; }
        public string StopType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public LocationDto Location { get; set; } = null!;

        public DateTime? EarliestTime { get; set; }
        public DateTime? LatestTime { get; set; }
        public DateTime? PlannedTime { get; set; }
        public int EstimatedDurationMinutes { get; set; }

        public decimal? PickupWeight { get; set; }
        public decimal? DeliveryWeight { get; set; }
        public decimal? PickupVolume { get; set; }
        public decimal? DeliveryVolume { get; set; }

        public string? LoadDescription { get; set; }
        public string? SpecialInstructions { get; set; }
        public List<string> SpecialRequirements { get; set; } = new();

        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Status tracking
        public DateTime? ActualArrivalTime { get; set; }
        public DateTime? ActualDepartureTime { get; set; }
        public string? CompletionNotes { get; set; }
        public bool HasSignature { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
    }
}
