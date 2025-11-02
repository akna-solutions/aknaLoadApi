using AknaLoad.Domain.Dtos;
using AknaLoad.Domain.Enums;


namespace AknaLoad.Domain.Dtos.Requests
{
    /// <summary>
    /// DTO for creating loads - supports both single-stop and multi-stop
    /// </summary>
    public class CreateLoadRequest
    {
        // 🏢 Basic Load Information
        public long OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        // 📦 Load Properties
        public decimal Weight { get; set; }
        public decimal? Volume { get; set; }
        public DimensionsDto? Dimensions { get; set; }
        public string LoadType { get; set; } = "GeneralCargo";
        public List<string>? SpecialRequirements { get; set; }

        // 🎯 Load Strategy
        public bool IsMultiStop { get; set; } = false;
        public string? RoutingStrategy { get; set; } = "Manual"; // Manual, Optimized, Sequential, Flexible

        // 📍 Single-Stop Fields (Legacy Support - used when IsMultiStop = false)
        public LocationDto? PickupLocation { get; set; }
        public LocationDto? DeliveryLocation { get; set; }
        public DateTime? PickupDateTime { get; set; }
        public DateTime? DeliveryDeadline { get; set; }
        public bool FlexiblePickup { get; set; } = false;
        public bool FlexibleDelivery { get; set; } = false;
        public string? PickupInstructions { get; set; }
        public string? DeliveryInstructions { get; set; }

        // 🛑 Multi-Stop Fields (used when IsMultiStop = true)
        public List<LoadStopDto>? LoadStops { get; set; }

        // 🗺️ Route Information (optional)
        public decimal? DistanceKm { get; set; }
        public int? EstimatedDurationMinutes { get; set; }

        // 📞 Contact Information
        public string? ContactPersonName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // 💰 Pricing (optional)
        public decimal? FixedPrice { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

       
        // 🔧 Helper methods for enum conversion
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
            if (string.IsNullOrEmpty(RoutingStrategy))
                return LoadRoutingStrategy.Manual;

            return Enum.TryParse<LoadRoutingStrategy>(RoutingStrategy, true, out var result) ? result : LoadRoutingStrategy.Manual;
        }

        // 🎯 Business Logic Methods
        public DateTime? GetEarliestPickupTime()
        {
            if (!IsMultiStop)
                return PickupDateTime;

            if (LoadStops == null || !LoadStops.Any())
                return null;

            var pickupStops = LoadStops.Where(s => s.StopType == "Pickup" || s.StopType == "Both");
            var earliestTimes = pickupStops.Where(s => s.EarliestTime.HasValue).Select(s => s.EarliestTime!.Value);

            return earliestTimes.Any() ? earliestTimes.Min() : null;
        }

        public DateTime? GetLatestDeliveryTime()
        {
            if (!IsMultiStop)
                return DeliveryDeadline;

            if (LoadStops == null || !LoadStops.Any())
                return null;

            var deliveryStops = LoadStops.Where(s => s.StopType == "Delivery" || s.StopType == "Both");
            var latestTimes = deliveryStops.Where(s => s.LatestTime.HasValue).Select(s => s.LatestTime!.Value);

            return latestTimes.Any() ? latestTimes.Max() : null;
        }

        public decimal GetTotalPickupWeight()
        {
            if (!IsMultiStop)
                return Weight;

            if (LoadStops == null || !LoadStops.Any())
                return 0;

            return LoadStops.Where(s => s.PickupWeight.HasValue).Sum(s => s.PickupWeight!.Value);
        }

        public decimal GetTotalDeliveryWeight()
        {
            if (!IsMultiStop)
                return Weight;

            if (LoadStops == null || !LoadStops.Any())
                return 0;

            return LoadStops.Where(s => s.DeliveryWeight.HasValue).Sum(s => s.DeliveryWeight!.Value);
        }

        public int GetTotalStops()
        {
            if (!IsMultiStop)
                return 2; // pickup + delivery

            return LoadStops?.Count ?? 0;
        }

        public bool RequiresSpecialVehicle()
        {
            var requirements = GetSpecialRequirements();
            return requirements.Any(r => r == SpecialRequirement.Refrigerated ||
                                        r == SpecialRequirement.Hazardous ||
                                        r == SpecialRequirement.Oversized);
        }

        public TimeSpan GetEstimatedTotalDuration()
        {
            if (!IsMultiStop)
            {
                return TimeSpan.FromMinutes(EstimatedDurationMinutes ?? 480); // 8 hours default
            }

            if (LoadStops == null || !LoadStops.Any())
                return TimeSpan.Zero;

            var totalMinutes = LoadStops.Sum(s => s.EstimatedDurationMinutes);
            // Add travel time between stops (simplified - 30 min between each stop)
            totalMinutes += (LoadStops.Count - 1) * 30;

            return TimeSpan.FromMinutes(totalMinutes);
        }

        public List<string> GetAllCities()
        {
            var cities = new List<string>();

            if (!IsMultiStop)
            {
                if (!string.IsNullOrEmpty(PickupLocation?.City))
                    cities.Add(PickupLocation.City);
                if (!string.IsNullOrEmpty(DeliveryLocation?.City))
                    cities.Add(DeliveryLocation.City);
            }
            else if (LoadStops != null)
            {
                cities.AddRange(LoadStops.Where(s => !string.IsNullOrEmpty(s.Location?.City))
                                         .Select(s => s.Location!.City!)
                                         .Distinct());
            }

            return cities.Distinct().ToList();
        }

        public bool IsLongDistance()
        {
            if (DistanceKm.HasValue)
                return DistanceKm.Value > 500; // 500km+

            // Check if cities are different
            var cities = GetAllCities();
            return cities.Count > 1;
        }

        public bool IsUrgent()
        {
            var totalTimeWindow = GetLatestDeliveryTime() - GetEarliestPickupTime();
            return totalTimeWindow?.TotalHours < 24; // Less than 24 hours
        }

        public string GetLoadSummary()
        {
            var type = IsMultiStop ? "Multi-Stop" : "Single-Stop";
            var cities = GetAllCities();
            var cityInfo = cities.Count > 1 ? $"{cities.First()} → {cities.Last()}" : cities.FirstOrDefault() ?? "Unknown";

            return $"{type} | {Weight}kg | {cityInfo} | {GetTotalStops()} stops";
        }
    }
    
}