using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Dtos
{
    /// <summary>
    /// DTO for individual load stops in multi-stop loads
    /// </summary>
    public class LoadStopDto
    {
        public int StopOrder { get; set; }
        public string StopType { get; set; } = "Pickup"; // Pickup, Delivery, Both

        // 📍 Location
        public LocationDto Location { get; set; } = null!;

        // ⏰ Timing
        public DateTime? EarliestTime { get; set; }
        public DateTime? LatestTime { get; set; }
        public DateTime? PlannedTime { get; set; }
        public int EstimatedDurationMinutes { get; set; } = 30;

        // 📦 Load Quantities
        public decimal? PickupWeight { get; set; }
        public decimal? DeliveryWeight { get; set; }
        public decimal? PickupVolume { get; set; }
        public decimal? DeliveryVolume { get; set; }

        // 📋 Instructions
        public string? LoadDescription { get; set; }
        public string? SpecialInstructions { get; set; }
        public List<string>? SpecialRequirements { get; set; }

        // 📞 Contact
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // 🔧 Helper methods
        public LoadStopType GetStopType()
        {
            return Enum.TryParse<LoadStopType>(StopType, true, out var result) ? result : LoadStopType.Pickup;
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



        public bool IsPickupStop => GetStopType() == LoadStopType.Pickup || GetStopType() == LoadStopType.Both;
        public bool IsDeliveryStop => GetStopType() == LoadStopType.Delivery || GetStopType() == LoadStopType.Both;

        public decimal GetTotalPickupWeight() => PickupWeight ?? 0;
        public decimal GetTotalDeliveryWeight() => DeliveryWeight ?? 0;

        public string GetStopSummary()
        {
            var type = StopType;
            var weight = IsPickupStop ? $"↑{GetTotalPickupWeight()}kg" : $"↓{GetTotalDeliveryWeight()}kg";
            return $"{type} | {weight} | {Location?.City}";
        }
    }
}