namespace AknaLoad.Domain.Dtos
{
    /// <summary>
    /// Response for multi-stop load with all stops
    /// </summary>
    public class MultiStopLoadResponseDto
    {
        public long Id { get; set; }
        public string LoadCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;

        public bool IsMultiStop { get; set; }
        public string RoutingStrategy { get; set; } = string.Empty;
        public int TotalStops { get; set; }

        public decimal Weight { get; set; }
        public decimal? Volume { get; set; }
        public string LoadType { get; set; } = string.Empty;
        public List<string> SpecialRequirements { get; set; } = new();

        public decimal? TotalDistanceKm { get; set; }
        public int? EstimatedTotalDurationMinutes { get; set; }
        public DateTime? EarliestPickupTime { get; set; }
        public DateTime? LatestDeliveryTime { get; set; }

        public decimal? FixedPrice { get; set; }

        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        public List<LoadStopResponseDto> LoadStops { get; set; } = new();

        public DateTime CreatedDate { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? MatchedAt { get; set; }
    }
}
