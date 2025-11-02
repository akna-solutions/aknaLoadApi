using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("Loads")]
    public class Load : BaseEntity
    {
        public long OwnerId { get; set; } // Identity Service reference
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LoadStatus Status { get; set; } = LoadStatus.Draft;
        public string LoadCode { get; set; } = string.Empty; // Auto-generated unique code

        // Lokasyon Bilgileri
        public string PickupLocationJson { get; set; } = string.Empty; // JSON serialized Location
        public string DeliveryLocationJson { get; set; } = string.Empty; // JSON serialized Location
        public DateTime PickupDateTime { get; set; }
        public DateTime DeliveryDeadline { get; set; }
        public bool FlexiblePickup { get; set; } = false;
        public bool FlexibleDelivery { get; set; } = false;

        // Yük Özellikleri
        public decimal Weight { get; set; } // kg
        public decimal? Volume { get; set; } // m³
        public string? DimensionsJson { get; set; } // JSON serialized Dimensions
        public LoadType LoadType { get; set; } = LoadType.GeneralCargo;
        public string? SpecialRequirementsJson { get; set; } // JSON array of SpecialRequirement

        // Fiyat ve Eşleştirme
        public decimal? FixedPrice { get; set; } // algoritma tarafından belirlenen
        public string? PricingFactorsJson { get; set; } // JSON of pricing calculation factors
        public decimal? DistanceKm { get; set; }
        public int? EstimatedDurationMinutes { get; set; }

        // İletişim ve Talimatlar
        public string? PickupInstructions { get; set; }
        public string? DeliveryInstructions { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Metadata
        public DateTime? PublishedAt { get; set; }
        public DateTime? MatchedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? MatchedDriverId { get; set; }
        public long? MatchedVehicleId { get; set; }

        // Navigation Properties (not mapped to database)
        [NotMapped]
        public Location? PickupLocation
        {
            get => string.IsNullOrEmpty(PickupLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(PickupLocationJson);
            set => PickupLocationJson = value == null ? string.Empty :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Location? DeliveryLocation
        {
            get => string.IsNullOrEmpty(DeliveryLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(DeliveryLocationJson);
            set => DeliveryLocationJson = value == null ? string.Empty :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Dimensions? Dimensions
        {
            get => string.IsNullOrEmpty(DimensionsJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Dimensions>(DimensionsJson);
            set => DimensionsJson = value == null ? null :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<SpecialRequirement> SpecialRequirements
        {
            get => string.IsNullOrEmpty(SpecialRequirementsJson) ? new List<SpecialRequirement>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<SpecialRequirement>>(SpecialRequirementsJson) ?? new List<SpecialRequirement>();
            set => SpecialRequirementsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}