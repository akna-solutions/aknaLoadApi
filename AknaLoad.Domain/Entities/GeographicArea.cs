using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("GeographicAreas")]
    public class GeographicArea : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GeographicAreaType AreaType { get; set; }

        // Geographic Definition
        public string? CenterLocationJson { get; set; } // JSON serialized Location for radius-based areas
        public decimal? RadiusKm { get; set; } // For radius-based areas
        public string? BoundaryPolygonJson { get; set; } // JSON array of Location points for polygon areas

        // Administrative Areas
        public string? Country { get; set; } = "TR";
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? PostalCodes { get; set; } // Comma-separated postal codes

        // Area Properties
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 1; // Higher numbers = higher priority
        public decimal? ServiceCostMultiplier { get; set; } = 1.0m; // Pricing adjustment for this area

        // Usage Statistics
        public int LoadCount { get; set; } = 0;
        public int DriverCount { get; set; } = 0;
        public DateTime? LastUsedAt { get; set; }

        // Navigation Properties (not mapped to database)
        [NotMapped]
        public Location? CenterLocation
        {
            get => string.IsNullOrEmpty(CenterLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(CenterLocationJson);
            set => CenterLocationJson = value == null ? null :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<Location> BoundaryPolygon
        {
            get => string.IsNullOrEmpty(BoundaryPolygonJson) ? new List<Location>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<Location>>(BoundaryPolygonJson) ?? new List<Location>();
            set => BoundaryPolygonJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        // Helper Methods
        public bool ContainsPoint(Location point)
        {
            switch (AreaType)
            {
                case GeographicAreaType.Radius:
                    if (CenterLocation != null && RadiusKm.HasValue)
                    {
                        return CenterLocation.DistanceTo(point) <= (double)RadiusKm.Value;
                    }
                    break;

                case GeographicAreaType.City:
                    return string.Equals(point.City, City, StringComparison.OrdinalIgnoreCase);

                case GeographicAreaType.Province:
                    return string.Equals(point.City, Province, StringComparison.OrdinalIgnoreCase);

                case GeographicAreaType.PostalCode:
                    return PostalCodes?.Split(',').Contains(point.PostalCode, StringComparer.OrdinalIgnoreCase) ?? false;

                case GeographicAreaType.Polygon:
                    return IsPointInPolygon(point, BoundaryPolygon);
            }

            return false;
        }

        private bool IsPointInPolygon(Location point, List<Location> polygon)
        {
            if (polygon.Count < 3) return false;

            bool inside = false;
            int j = polygon.Count - 1;

            for (int i = 0; i < polygon.Count; i++)
            {
                if (((polygon[i].Latitude > point.Latitude) != (polygon[j].Latitude > point.Latitude)) &&
                    (point.Longitude < (polygon[j].Longitude - polygon[i].Longitude) *
                     (point.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }
    }
}