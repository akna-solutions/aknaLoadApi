using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Load
{
    /// <summary>
    /// Lightweight DTO for load list view
    /// </summary>
    public class LoadListItemDto
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public LoadStatus Status { get; set; }
        public string LoadCode { get; set; } = string.Empty;

        // Multi-Stop Info
        public bool IsMultiStop { get; set; }
        public int TotalStops { get; set; }

        // Load Properties
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public LoadType LoadType { get; set; }

        // Route Summary
        public string? OriginCity { get; set; }
        public string? DestinationCity { get; set; }
        public decimal? TotalDistanceKm { get; set; }

        // Pricing
        public decimal? FixedPrice { get; set; }

        // Dates
        public DateTime? EarliestPickupTime { get; set; }
        public DateTime? LatestDeliveryTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }


        // Contact
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }
}
