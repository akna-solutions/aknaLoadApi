using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("Loads")]
    public class Load : BaseEntity
    {
        public long CompanyId { get; set; } // Identity Service reference
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LoadStatus Status { get; set; } = LoadStatus.Draft;
        public string LoadCode { get; set; } = string.Empty; // Auto-generated unique code

        // Multi-Stop Configuration
        public bool IsMultiStop { get; set; } = false;
        public LoadRoutingStrategy RoutingStrategy { get; set; } = LoadRoutingStrategy.Manual;
        public int TotalStops { get; set; } = 0;

        // Legacy Single Stop Support (Backward Compatibility)
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public string PickupLocationJson { get; set; } = string.Empty; // JSON serialized Location
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public string DeliveryLocationJson { get; set; } = string.Empty; // JSON serialized Location
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public DateTime PickupDateTime { get; set; }
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public DateTime DeliveryDeadline { get; set; }
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public bool FlexiblePickup { get; set; } = false;
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public bool FlexibleDelivery { get; set; } = false;
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public string? PickupInstructions { get; set; }
        [Obsolete("Use LoadStops collection for multi-stop support")]
        public string? DeliveryInstructions { get; set; }

        // Yük Özellikleri
        public decimal Weight { get; set; } // kg - toplam ağırlık
        public decimal? Volume { get; set; } // m³ - toplam hacim
        public string? DimensionsJson { get; set; } // JSON serialized Dimensions
        public LoadType LoadType { get; set; } = LoadType.GeneralCargo;
        public string? SpecialRequirementsJson { get; set; } // JSON array of SpecialRequirement

        // Route Information
        public decimal? TotalDistanceKm { get; set; }
        public int? EstimatedTotalDurationMinutes { get; set; }
        public DateTime? EarliestPickupTime { get; set; }  // En erken alım zamanı
        public DateTime? LatestDeliveryTime { get; set; }  // En geç teslimat zamanı

        // Fiyat ve Eşleştirme
        public decimal? FixedPrice { get; set; } // algoritma tarafından belirlenen
        public string? PricingFactorsJson { get; set; } // JSON of pricing calculation factors

        // Legacy fields (backward compatibility)
        [Obsolete("Use TotalDistanceKm for multi-stop loads")]
        public decimal? DistanceKm { get; set; }
        [Obsolete("Use EstimatedTotalDurationMinutes for multi-stop loads")]
        public int? EstimatedDurationMinutes { get; set; }

        // İletişim
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Metadata
        public DateTime? PublishedAt { get; set; }
        public DateTime? MatchedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? MatchedDriverId { get; set; }
        public long? MatchedVehicleId { get; set; }

        // Navigation Properties
        public virtual ICollection<LoadStop> LoadStops { get; set; } = new List<LoadStop>();
        public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
        public virtual ICollection<PricingCalculation> PricingCalculations { get; set; } = new List<PricingCalculation>();
        public virtual ICollection<LoadTracking> LoadTrackings { get; set; } = new List<LoadTracking>();

        // Helper Properties (not mapped to database)
        [NotMapped]
        [Obsolete("Use LoadStops[0].Location for multi-stop support")]
        public Location? PickupLocation
        {
            get => string.IsNullOrEmpty(PickupLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(PickupLocationJson);
            set => PickupLocationJson = value == null ? string.Empty :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        [Obsolete("Use LoadStops.Last().Location for multi-stop support")]
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

        // Multi-Stop Helper Methods
        public List<LoadStop> GetPickupStops() => LoadStops.Where(s => s.IsPickupStop).OrderBy(s => s.StopOrder).ToList();
        public List<LoadStop> GetDeliveryStops() => LoadStops.Where(s => s.IsDeliveryStop).OrderBy(s => s.StopOrder).ToList();
        public LoadStop? GetFirstStop() => LoadStops.OrderBy(s => s.StopOrder).FirstOrDefault();
        public LoadStop? GetLastStop() => LoadStops.OrderByDescending(s => s.StopOrder).FirstOrDefault();

        public decimal GetTotalPickupWeight() => LoadStops.Sum(s => s.GetTotalPickupWeight());
        public decimal GetTotalDeliveryWeight() => LoadStops.Sum(s => s.GetTotalDeliveryWeight());

        public bool ValidateStopOrder()
        {
            var stops = LoadStops.OrderBy(s => s.StopOrder).ToList();
            for (int i = 0; i < stops.Count; i++)
            {
                if (stops[i].StopOrder != i + 1)
                    return false;
            }
            return true;
        }

        public TimeSpan GetTotalTimeWindow()
        {
            if (!LoadStops.Any()) return TimeSpan.Zero;

            var stopsWithEarliest = LoadStops.Where(s => s.EarliestTime.HasValue).ToList();
            var stopsWithLatest = LoadStops.Where(s => s.LatestTime.HasValue).ToList();

            if (!stopsWithEarliest.Any() || !stopsWithLatest.Any())
                return TimeSpan.Zero;

            var earliest = stopsWithEarliest.Min(s => s.EarliestTime!.Value);
            var latest = stopsWithLatest.Max(s => s.LatestTime!.Value);

            return latest - earliest;
        }

        // Backward Compatibility Methods
        public void ConvertToMultiStop()
        {
            if (IsMultiStop) return;

            LoadStops.Clear();

            // Create pickup stop
            if (PickupLocation != null)
            {
                LoadStops.Add(new LoadStop
                {
                    LoadId = Id,
                    StopOrder = 1,
                    StopType = LoadStopType.Pickup,
                    Location = PickupLocation,
                    EarliestTime = PickupDateTime.AddHours(-1),
                    LatestTime = PickupDateTime.AddHours(1),
                    PlannedTime = PickupDateTime,
                    PickupWeight = Weight,
                    PickupVolume = Volume,
                    SpecialInstructions = PickupInstructions,
                    SpecialRequirements = SpecialRequirements
                });
            }

            // Create delivery stop
            if (DeliveryLocation != null)
            {
                LoadStops.Add(new LoadStop
                {
                    LoadId = Id,
                    StopOrder = 2,
                    StopType = LoadStopType.Delivery,
                    Location = DeliveryLocation,
                    EarliestTime = DeliveryDeadline.AddHours(-2),
                    LatestTime = DeliveryDeadline,
                    PlannedTime = DeliveryDeadline.AddHours(-1),
                    DeliveryWeight = Weight,
                    DeliveryVolume = Volume,
                    SpecialInstructions = DeliveryInstructions,
                    SpecialRequirements = SpecialRequirements
                });
            }

            IsMultiStop = true;
            TotalStops = LoadStops.Count;
            TotalDistanceKm = DistanceKm;
            EstimatedTotalDurationMinutes = EstimatedDurationMinutes;
            EarliestPickupTime = PickupDateTime;
            LatestDeliveryTime = DeliveryDeadline;
        }
    }
}