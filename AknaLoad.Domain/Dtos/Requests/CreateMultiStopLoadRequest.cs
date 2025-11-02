using AknaLoad.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AknaLoad.Domain.Dtos.Requests
{
    /// <summary>
    /// Request for creating multi-stop loads
    /// </summary>
    public class CreateMultiStopLoadRequest
    {
        public long OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Load Properties
        public decimal Weight { get; set; }
        public decimal? Volume { get; set; }
        public DimensionsDto? Dimensions { get; set; }
        public string LoadType { get; set; } = "GeneralCargo";
        public List<string>? SpecialRequirements { get; set; }

        // Multi-Stop Configuration
        public string RoutingStrategy { get; set; } = "Manual"; // Manual, Optimized, Sequential, Flexible
        public List<LoadStopDto> LoadStops { get; set; } = new();

        // Contact Information
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Helper methods for enum conversion
        public LoadType GetLoadType()
        {
            Enum.TryParse<LoadType>(LoadType, true, out var result);
            return result;
        }

        public List<SpecialRequirement> GetSpecialRequirements()
        {
            if (SpecialRequirements == null || !SpecialRequirements.Any())
                return new List<SpecialRequirement>();

            var requirements = new List<SpecialRequirement>();
            foreach (var req in SpecialRequirements)
            {
                if (Enum.TryParse<SpecialRequirement>(req, true, out var result))
                {
                    requirements.Add(result);
                }
            }
            return requirements;
        }

        public LoadRoutingStrategy GetRoutingStrategy()
        {
            return Enum.TryParse<LoadRoutingStrategy>(RoutingStrategy, true, out var result) ? result : LoadRoutingStrategy.Manual;
        }
    }
}
