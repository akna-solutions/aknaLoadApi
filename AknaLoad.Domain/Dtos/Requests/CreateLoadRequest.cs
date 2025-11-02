using AknaLoad.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AknaLoad.Domain.Dtos.Requests
{
    public class CreateLoadRequest
    {
        public long OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Lokasyon Bilgileri
        public LocationDto PickupLocation { get; set; } = null!;
        public LocationDto DeliveryLocation { get; set; } = null!;
        public DateTime PickupDateTime { get; set; }
        public DateTime DeliveryDeadline { get; set; }
        public bool FlexiblePickup { get; set; } = false;
        public bool FlexibleDelivery { get; set; } = false;

        // Yük Özellikleri
        public decimal Weight { get; set; }
        public decimal? Volume { get; set; }
        public DimensionsDto? Dimensions { get; set; }
        public LoadType LoadType { get; set; } = LoadType.GeneralCargo;
        public List<SpecialRequirement>? SpecialRequirements { get; set; }

        // Mesafe ve Süre (opsiyonel)
        public decimal? DistanceKm { get; set; }
        public int? EstimatedDurationMinutes { get; set; }

        // İletişim ve Talimatlar
        public string? PickupInstructions { get; set; }
        public string? DeliveryInstructions { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
    }
}
