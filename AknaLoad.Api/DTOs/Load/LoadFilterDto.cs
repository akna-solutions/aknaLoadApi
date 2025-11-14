using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Load
{
    /// <summary>
    /// DTO for filtering loads
    /// </summary>
    public class LoadFilterDto
    {
        public long? CompanyId { get; set; }

        // Status Filters
        public LoadStatus? Status { get; set; }
        public List<LoadStatus>? Statuses { get; set; }

        // Date Filters
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public DateTime? PickupFrom { get; set; }
        public DateTime? PickupTo { get; set; }
        public DateTime? DeliveryFrom { get; set; }
        public DateTime? DeliveryTo { get; set; }

        // Load Type Filter
        public LoadType? LoadType { get; set; }

        // Multi-Stop Filter
        public bool? IsMultiStop { get; set; }

        // Location Filters
        public string? OriginCity { get; set; }
        public string? DestinationCity { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
    }
}
