using AknaLoad.Domain.Entities.BaseEnities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace AknaLoad.Domain.Entities
{
    [Table("Drivers")]
    public class Driver : BaseEntity
    {
        public long UserId { get; set; } // Identity Service reference
        public long CompanyId { get; set; } // Identity Service reference
        public string DriverCode { get; set; } = string.Empty; // Auto-generated unique code

        // Ehliyet ve Deneyim
        public string LicenseNumber { get; set; } = string.Empty;
        public string LicenseCategory { get; set; } = string.Empty; // B, C, C+E, etc.
        public int ExperienceYears { get; set; } = 0;
        public DateTime? LicenseExpiryDate { get; set; }

        // Konum ve Availability
        public string? CurrentLocationJson { get; set; } // JSON serialized Location
        public string? HomeBaseJson { get; set; } // JSON serialized Location
        public DriverAvailabilityStatus Status { get; set; } = DriverAvailabilityStatus.Available;
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }
        public string? WorkingHoursJson { get; set; } // JSON serialized WorkingHours
        public int MaxDistanceKm { get; set; } = 500; // Maximum willing to travel

        // Performans ve Değerlendirme
        public int CompletedLoads { get; set; } = 0;
        public decimal AverageRating { get; set; } = 0;
        public decimal OnTimePercentage { get; set; } = 100;
        public decimal CancellationRate { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;

        // Hizmet Alanları
        public string? ServiceAreasJson { get; set; } // JSON array of geographic areas
        public string? PreferredRoutesJson { get; set; } // JSON array of preferred routes

        // Özel Yetenekler ve Sertifikalar
        public bool HasADRLicense { get; set; } = false;
        public bool HasSRCLicense { get; set; } = false;
        public bool HasForkliftLicense { get; set; } = false;
        public string? SpecialSkillsJson { get; set; } // JSON array of special skills

        // Araç Bilgileri
        public long? CurrentVehicleId { get; set; }
        public string? VehicleIds { get; set; } // JSON array of vehicle IDs this driver can operate

        // İletişim Tercihleri
        public bool AcceptsSMSNotifications { get; set; } = true;
        public bool AcceptsPushNotifications { get; set; } = true;
        public bool AcceptsEmailNotifications { get; set; } = true;

        // Son Aktivite
        public DateTime? LastActiveAt { get; set; }
        public DateTime? LastLocationUpdateAt { get; set; }

        // Navigation Properties (not mapped to database)
        [NotMapped]
        public Location? CurrentLocation
        {
            get => string.IsNullOrEmpty(CurrentLocationJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(CurrentLocationJson);
            set => CurrentLocationJson = value == null ? null :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Location? HomeBase
        {
            get => string.IsNullOrEmpty(HomeBaseJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<Location>(HomeBaseJson);
            set => HomeBaseJson = value == null ? null :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public WorkingHours? WorkingHours
        {
            get => string.IsNullOrEmpty(WorkingHoursJson) ? null :
                   System.Text.Json.JsonSerializer.Deserialize<WorkingHours>(WorkingHoursJson);
            set => WorkingHoursJson = value == null ? null :
                   System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}