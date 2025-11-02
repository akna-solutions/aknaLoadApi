
namespace AknaLoad.Domain.Dtos
{
    /// <summary>
    /// DTO for Vehicle information from Identity Service
    /// </summary>
    public class VehicleDto
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public long? CurrentDriverId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? ModelYear { get; set; }

        // Vehicle Type & Body
        public string VehicleType { get; set; } = string.Empty; // From enum
        public string BodyType { get; set; } = string.Empty;     // From enum
        public string Status { get; set; } = string.Empty;      // From enum

        // Capacity Information
        public int? PayloadCapacity { get; set; }     // kg
        public decimal? CargoVolume { get; set; }     // m3
        public decimal? CargoInnerLengthM { get; set; }
        public decimal? CargoInnerWidthM { get; set; }
        public decimal? CargoInnerHeightM { get; set; }

        // Special Capabilities
        public bool HasLiftgate { get; set; }
        public bool HasCrane { get; set; }
        public bool IsRefrigerated { get; set; }
        public bool HazmatAllowed { get; set; }
        public bool CanCarryContainer { get; set; }

        // Current Status
        public decimal? LastKnownLat { get; set; }
        public decimal? LastKnownLng { get; set; }
        public DateTime? LastLocationAt { get; set; }

        // Helper Methods
        public bool CanCarryWeight(decimal weightKg)
        {
            return PayloadCapacity.HasValue && PayloadCapacity.Value >= weightKg;
        }

        public bool CanCarryVolume(decimal volumeM3)
        {
            return CargoVolume.HasValue && CargoVolume.Value >= volumeM3;
        }

        public bool CanCarryDimensions(decimal length, decimal width, decimal height)
        {
            return CargoInnerLengthM.HasValue && CargoInnerLengthM.Value >= length &&
                   CargoInnerWidthM.HasValue && CargoInnerWidthM.Value >= width &&
                   CargoInnerHeightM.HasValue && CargoInnerHeightM.Value >= height;
        }
    }
}
