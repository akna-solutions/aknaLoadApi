using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("LoadStops")]
    public class LoadStop : BaseEntity
    {
        // 🔗 İlişki Bilgileri
        public long LoadId { get; set; }
        public int StopOrder { get; set; } // 1, 2, 3... (sıralama için)
        public LoadStopType StopType { get; set; } // Pickup, Delivery, Both

        // 📍 Lokasyon Bilgileri
        public string LocationJson { get; set; } = string.Empty; // JSON serialized Location

        // ⏰ Zaman Yönetimi
        public DateTime? EarliestTime { get; set; } // En erken varış
        public DateTime? LatestTime { get; set; }   // En geç varış
        public DateTime? PlannedTime { get; set; }  // Planlanan zaman
        public int EstimatedDurationMinutes { get; set; } = 30; // Bu duraksış süre

        // 📦 Yük Miktarları
        public decimal? PickupWeight { get; set; }    // Bu noktada alınacak ağırlık
        public decimal? DeliveryWeight { get; set; }  // Bu noktada bırakılacak ağırlık
        public decimal? PickupVolume { get; set; }    // Bu noktada alınacak hacim
        public decimal? DeliveryVolume { get; set; }  // Bu noktada bırakılacak hacim

        // 📋 Yük Detayları
        public string? LoadDescription { get; set; }  // Bu noktadaki yük açıklaması
        public string? SpecialInstructions { get; set; } // Özel talimatlar
        public string? SpecialRequirementsJson { get; set; } // JSON array of requirements

        // 📞 İletişim
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // 📊 Durum Takibi
        public LoadStopStatus Status { get; set; } = LoadStopStatus.Planned;
        public DateTime? ActualArrivalTime { get; set; }
        public DateTime? ActualDepartureTime { get; set; }
        public string? CompletionNotes { get; set; }
        public string? SignatureUrl { get; set; } // Dijital imza
        public string? PhotoUrlsJson { get; set; } // Fotoğraf URL'leri

        // 🔗 Navigation Properties
        public virtual Load Load { get; set; } = null!;

        // 🛠️ Helper Properties (not mapped to database)
        [NotMapped]
        public Location? Location
        {
            get => string.IsNullOrEmpty(LocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(LocationJson);
            set => LocationJson = value == null ? string.Empty :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<SpecialRequirement> SpecialRequirements
        {
            get => string.IsNullOrEmpty(SpecialRequirementsJson) ? new List<SpecialRequirement>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<SpecialRequirement>>(SpecialRequirementsJson) ?? new List<SpecialRequirement>();
            set => SpecialRequirementsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> PhotoUrls
        {
            get => string.IsNullOrEmpty(PhotoUrlsJson) ? new List<string>() :
                   System.Text.Json.JsonSerializer.Deserialize<List<string>>(PhotoUrlsJson) ?? new List<string>();
            set => PhotoUrlsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        // ✅ Helper Methods
        public bool IsPickupStop => StopType == LoadStopType.Pickup || StopType == LoadStopType.Both;
        public bool IsDeliveryStop => StopType == LoadStopType.Delivery || StopType == LoadStopType.Both;

        public decimal GetTotalPickupWeight() => PickupWeight ?? 0;
        public decimal GetTotalDeliveryWeight() => DeliveryWeight ?? 0;
        public decimal GetTotalPickupVolume() => PickupVolume ?? 0;
        public decimal GetTotalDeliveryVolume() => DeliveryVolume ?? 0;

        public bool IsWithinTimeWindow(DateTime checkTime)
        {
            if (EarliestTime.HasValue && checkTime < EarliestTime.Value)
                return false;
            if (LatestTime.HasValue && checkTime > LatestTime.Value)
                return false;
            return true;
        }

        // 🎯 Advanced Helper Methods
        public TimeSpan GetTimeWindow()
        {
            if (!EarliestTime.HasValue || !LatestTime.HasValue)
                return TimeSpan.Zero;
            return LatestTime.Value - EarliestTime.Value;
        }

        public TimeSpan GetEstimatedDuration()
        {
            return TimeSpan.FromMinutes(EstimatedDurationMinutes);
        }

        public DateTime? GetExpectedDepartureTime()
        {
            if (!ActualArrivalTime.HasValue)
                return PlannedTime?.AddMinutes(EstimatedDurationMinutes);
            return ActualArrivalTime.Value.AddMinutes(EstimatedDurationMinutes);
        }

        public bool IsCompleted()
        {
            return Status == LoadStopStatus.Completed;
        }

        public bool IsDelayed(DateTime currentTime)
        {
            if (!PlannedTime.HasValue)
                return false;

            return currentTime > PlannedTime.Value.AddMinutes(15); // 15 dakika tolerans
        }

        public bool HasLoadChange()
        {
            return (PickupWeight.HasValue && PickupWeight > 0) ||
                   (DeliveryWeight.HasValue && DeliveryWeight > 0);
        }

        public decimal GetNetWeightChange()
        {
            return GetTotalPickupWeight() - GetTotalDeliveryWeight();
        }

        public decimal GetNetVolumeChange()
        {
            return GetTotalPickupVolume() - GetTotalDeliveryVolume();
        }

        public bool RequiresSignature()
        {
            return IsDeliveryStop &&
                   (SpecialRequirements.Contains(SpecialRequirement.HighValue) ||
                    SpecialRequirements.Contains(SpecialRequirement.DocumentsRequired));
        }

        public bool IsOnTime(DateTime currentTime)
        {
            return IsWithinTimeWindow(currentTime) && !IsDelayed(currentTime);
        }

        public string GetStopSummary()
        {
            var parts = new List<string>();

            if (IsPickupStop)
                parts.Add($"Pickup: {GetTotalPickupWeight()}kg");

            if (IsDeliveryStop)
                parts.Add($"Delivery: {GetTotalDeliveryWeight()}kg");

            return string.Join(" | ", parts);
        }

        public bool CanStartOperation(DateTime currentTime)
        {
            if (Status != LoadStopStatus.Arrived)
                return false;

            return IsWithinTimeWindow(currentTime);
        }

        public void StartOperation(string operatedBy)
        {
            if (Status == LoadStopStatus.Arrived)
            {
                Status = LoadStopStatus.Loading;
                UpdatedUser = operatedBy;
            }
        }

        public void CompleteOperation(string completedBy, string? notes = null, string? signatureUrl = null)
        {
            Status = LoadStopStatus.Completed;
            ActualDepartureTime = DateTime.UtcNow;
            CompletionNotes = notes;
            SignatureUrl = signatureUrl;
            UpdatedUser = completedBy;
        }

        public void ReportDelay(string reason, string reportedBy)
        {
            Status = LoadStopStatus.Delayed;
            CompletionNotes = $"Delay reported: {reason}";
            UpdatedUser = reportedBy;
        }

        public void AddPhoto(string photoUrl, string addedBy)
        {
            var photos = PhotoUrls.ToList();
            photos.Add(photoUrl);
            PhotoUrls = photos;
            UpdatedUser = addedBy;
        }

        public ValidationResult ValidateStop()
        {
            var result = new ValidationResult();

            // Time window validation
            if (EarliestTime.HasValue && LatestTime.HasValue && EarliestTime >= LatestTime)
                result.AddError("Earliest time must be before latest time");

            // Weight validation
            if (PickupWeight.HasValue && PickupWeight < 0)
                result.AddError("Pickup weight cannot be negative");

            if (DeliveryWeight.HasValue && DeliveryWeight < 0)
                result.AddError("Delivery weight cannot be negative");

            // Stop type validation
            if (StopType == LoadStopType.Pickup && (!PickupWeight.HasValue || PickupWeight <= 0))
                result.AddError("Pickup stops must have pickup weight");

            if (StopType == LoadStopType.Delivery && (!DeliveryWeight.HasValue || DeliveryWeight <= 0))
                result.AddError("Delivery stops must have delivery weight");

            // Location validation
            if (Location == null)
                result.AddError("Location is required");

            return result;
        }
    }

    // Validation helper class
    public class ValidationResult
    {
        public List<string> Errors { get; set; } = new();
        public bool IsValid => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}