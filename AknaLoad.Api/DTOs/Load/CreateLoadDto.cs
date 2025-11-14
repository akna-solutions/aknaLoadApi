using AknaLoad.Domain.Enums;

namespace AknaLoad.Api.DTOs.Load
{
    /// <summary>
    /// DTO for creating a new load (single or multi-stop)
    /// </summary>
    public class CreateLoadDto
    {
        public long CompanyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Multi-Stop Configuration
        public bool IsMultiStop { get; set; } = false;
        public LoadRoutingStrategy RoutingStrategy { get; set; } = LoadRoutingStrategy.Manual;
        public List<LoadStopDto> LoadStops { get; set; } = new();

        // Load Properties
        public decimal Weight { get; set; }
        public decimal? Volume { get; set; }
        public DimensionsDto? Dimensions { get; set; }
        public LoadType LoadType { get; set; } = LoadType.GeneralCargo;
        public List<SpecialRequirement>? SpecialRequirements { get; set; }

        // Pricing
        public decimal? FixedPrice { get; set; }

        // Contact
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }
}
