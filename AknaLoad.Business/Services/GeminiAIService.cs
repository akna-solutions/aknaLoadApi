using AknaLoad.Domain.Enums;
using GenerativeAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AknaLoad.Application.Services
{
    public interface IGeminiAIService
    {
        Task<decimal> OptimizePricingAsync(decimal basePrice, decimal distance, decimal weight,
            decimal? volume, LoadType loadType, List<SpecialRequirement> specialRequirements,
            DateTime pickupTime, DateTime deliveryTime);

        Task<List<VehicleRecommendation>> GetVehicleRecommendationsAsync(decimal weight,
            decimal? volume, LoadType loadType, List<SpecialRequirement> specialRequirements,
            decimal? length = null, decimal? width = null, decimal? height = null);

        Task<RouteCalculationResult> CalculateRouteAsync(List<RouteStopInfo> stops);
    }

    public class GeminiAIService : IGeminiAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly string _apiKey;

        public GeminiAIService(IConfiguration configuration, ILogger<GeminiAIService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        }

        public async Task<decimal> OptimizePricingAsync(
            decimal basePrice,
            decimal distance,
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements,
            DateTime pickupTime,
            DateTime deliveryTime)
        {
            try
            {
                var model = new GenerativeModel(apiKey: _apiKey, model: "gemini-1.5-flash");

                var prompt = $@"Sen bir lojistik fiyatlandırma uzmanısın. Aşağıdaki yük için fiyat optimizasyonu yap:

Base Fiyat: {basePrice:F2} TL
Mesafe: {distance:F2} km
Ağırlık: {weight:F2} kg
Hacim: {(volume.HasValue ? $"{volume.Value:F2} m³" : "Belirtilmemiş")}
Yük Tipi: {loadType}
Özel Gereksinimler: {string.Join(", ", specialRequirements)}
Alım Tarihi: {pickupTime:dd.MM.yyyy HH:mm}
Teslimat Tarihi: {deliveryTime:dd.MM.yyyy HH:mm}

Görevler:
1. Piyasa koşullarını değerlendir
2. Yük özelliklerine göre risk analizi yap
3. Zaman faktörünü değerlendir (aciliyet, hafta sonu, peak hours)
4. Rekabetçi bir fiyat öner

SADECE optimized price değerini sayısal olarak ver (örnek: 1250.50). Başka açıklama yapma.";

                var response = await model.GenerateContentAsync(prompt);
                var resultText = response.Text.Trim();

                // Extract numeric value from response
                var cleanedText = new string(resultText.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
                cleanedText = cleanedText.Replace(',', '.');

                if (decimal.TryParse(cleanedText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal optimizedPrice))
                {
                    // Ensure optimized price is within reasonable bounds (50% to 200% of base price)
                    var minPrice = basePrice * 0.5m;
                    var maxPrice = basePrice * 2.0m;

                    optimizedPrice = Math.Max(minPrice, Math.Min(maxPrice, optimizedPrice));

                    _logger.LogInformation("AI optimized price from {BasePrice} to {OptimizedPrice}", basePrice, optimizedPrice);
                    return optimizedPrice;
                }

                _logger.LogWarning("Could not parse AI response, returning base price. Response: {Response}", resultText);
                return basePrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini AI for price optimization");
                return basePrice; // Fallback to base price on error
            }
        }

        public async Task<List<VehicleRecommendation>> GetVehicleRecommendationsAsync(
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements,
            decimal? length = null,
            decimal? width = null,
            decimal? height = null)
        {
            try
            {
                var model = new GenerativeModel(apiKey: _apiKey, model: "gemini-1.5-flash");

                var dimensionsInfo = length.HasValue && width.HasValue && height.HasValue
                    ? $"Boyutlar: {length.Value}m x {width.Value}m x {height.Value}m"
                    : "Boyutlar belirtilmemiş";

                var prompt = $@"Sen bir lojistik araç eşleştirme uzmanısın. Aşağıdaki yük için en uygun araç tiplerini öner:

Ağırlık: {weight:F2} kg
Hacim: {(volume.HasValue ? $"{volume.Value:F2} m³" : "Belirtilmemiş")}
{dimensionsInfo}
Yük Tipi: {loadType}
Özel Gereksinimler: {string.Join(", ", specialRequirements)}

Türkiye'deki yaygın araç tipleri:
1. Van/Panelvan (500-1000 kg, 3-7 m³)
2. Kamyonet (1000-3500 kg, 8-15 m³)
3. Tır (3500+ kg, 15+ m³)
4. Frigorifik Van (soğutmalı, 500-1000 kg)
5. Frigorifik Kamyonet (soğutmalı, 1000-3500 kg)
6. Açık Kasa (oversized yükler için)
7. ADR Sertifikalı (tehlikeli maddeler için)

SADECE JSON formatında yanıt ver:
[
  {{
    ""vehicleType"": ""araç tipi"",
    ""suitabilityScore"": 0-100 arası puan,
    ""reason"": ""seçim nedeni"",
    ""maxWeight"": kapasite kg,
    ""maxVolume"": kapasite m³,
    ""estimatedCost"": tahmini maliyet TL
  }}
]

En az 2, en fazla 4 araç tipi öner. En uygun olandan başla.";

                var response = await model.GenerateContentAsync(prompt);
                var resultText = response.Text.Trim();

                // Remove markdown code blocks if present
                if (resultText.StartsWith("```json"))
                {
                    resultText = resultText.Substring(7);
                }
                if (resultText.StartsWith("```"))
                {
                    resultText = resultText.Substring(3);
                }
                if (resultText.EndsWith("```"))
                {
                    resultText = resultText.Substring(0, resultText.Length - 3);
                }
                resultText = resultText.Trim();

                var recommendations = JsonSerializer.Deserialize<List<VehicleRecommendation>>(resultText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (recommendations != null && recommendations.Any())
                {
                    _logger.LogInformation("AI recommended {Count} vehicle types", recommendations.Count);
                    return recommendations;
                }

                _logger.LogWarning("Could not parse AI response for vehicle recommendations");
                return GetFallbackVehicleRecommendations(weight, volume, loadType, specialRequirements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini AI for vehicle recommendations");
                return GetFallbackVehicleRecommendations(weight, volume, loadType, specialRequirements);
            }
        }

        private List<VehicleRecommendation> GetFallbackVehicleRecommendations(
            decimal weight,
            decimal? volume,
            LoadType loadType,
            List<SpecialRequirement> specialRequirements)
        {
            var recommendations = new List<VehicleRecommendation>();

            var isRefrigerated = specialRequirements.Contains(SpecialRequirement.Refrigerated) ||
                                specialRequirements.Contains(SpecialRequirement.ColdChain);
            var isHazardous = specialRequirements.Contains(SpecialRequirement.Hazardous);
            var isOversized = specialRequirements.Contains(SpecialRequirement.Oversized);

            // Rule-based fallback logic
            if (isHazardous)
            {
                recommendations.Add(new VehicleRecommendation
                {
                    VehicleType = "ADR Sertifikalı Tır",
                    SuitabilityScore = 95,
                    Reason = "Tehlikeli madde taşımacılığı için ADR sertifikası gereklidir",
                    MaxWeight = 24000,
                    MaxVolume = 90,
                    EstimatedCost = weight * 5.0m
                });
            }
            else if (isOversized)
            {
                recommendations.Add(new VehicleRecommendation
                {
                    VehicleType = "Açık Kasa Tır",
                    SuitabilityScore = 90,
                    Reason = "Gabaritli yükler için açık kasa tercih edilir",
                    MaxWeight = 24000,
                    MaxVolume = null,
                    EstimatedCost = weight * 4.5m
                });
            }
            else if (weight <= 1000)
            {
                if (isRefrigerated)
                {
                    recommendations.Add(new VehicleRecommendation
                    {
                        VehicleType = "Frigorifik Van",
                        SuitabilityScore = 95,
                        Reason = "Hafif yükler için soğutmalı van uygun",
                        MaxWeight = 1000,
                        MaxVolume = 7,
                        EstimatedCost = weight * 3.5m
                    });
                }
                else
                {
                    recommendations.Add(new VehicleRecommendation
                    {
                        VehicleType = "Van/Panelvan",
                        SuitabilityScore = 90,
                        Reason = "Hafif yükler için ekonomik seçenek",
                        MaxWeight = 1000,
                        MaxVolume = 7,
                        EstimatedCost = weight * 2.0m
                    });
                }
            }
            else if (weight <= 3500)
            {
                if (isRefrigerated)
                {
                    recommendations.Add(new VehicleRecommendation
                    {
                        VehicleType = "Frigorifik Kamyonet",
                        SuitabilityScore = 95,
                        Reason = "Orta ağırlıktaki yükler için soğutmalı kamyonet",
                        MaxWeight = 3500,
                        MaxVolume = 15,
                        EstimatedCost = weight * 3.0m
                    });
                }
                else
                {
                    recommendations.Add(new VehicleRecommendation
                    {
                        VehicleType = "Kamyonet",
                        SuitabilityScore = 90,
                        Reason = "Orta ağırlıktaki yükler için ideal",
                        MaxWeight = 3500,
                        MaxVolume = 15,
                        EstimatedCost = weight * 2.5m
                    });
                }
            }
            else
            {
                if (isRefrigerated)
                {
                    recommendations.Add(new VehicleRecommendation
                    {
                        VehicleType = "Frigorifik Tır",
                        SuitabilityScore = 95,
                        Reason = "Ağır yükler için soğutmalı tır",
                        MaxWeight = 24000,
                        MaxVolume = 90,
                        EstimatedCost = weight * 4.0m
                    });
                }
                else
                {
                    recommendations.Add(new VehicleRecommendation
                    {
                        VehicleType = "Tır",
                        SuitabilityScore = 90,
                        Reason = "Ağır yükler için standart tır",
                        MaxWeight = 24000,
                        MaxVolume = 90,
                        EstimatedCost = weight * 3.5m
                    });
                }
            }

            return recommendations;
        }

        public async Task<RouteCalculationResult> CalculateRouteAsync(List<RouteStopInfo> stops)
        {
            try
            {
                if (stops == null || stops.Count < 2)
                {
                    return new RouteCalculationResult
                    {
                        TotalDistanceKm = 0,
                        EstimatedDurationMinutes = 0,
                        Success = false,
                        ErrorMessage = "En az 2 durak gereklidir"
                    };
                }

                var model = new GenerativeModel(apiKey: _apiKey, model: "gemini-1.5-flash");

                var stopsInfo = string.Join("\n", stops.Select((s, i) =>
                    $"{i + 1}. {s.City}, {s.District} (Enlem: {s.Latitude}, Boylam: {s.Longitude})"));

                var prompt = $@"Sen bir lojistik rota planlama uzmanısın. Aşağıdaki duraklar arasındaki toplam mesafeyi ve tahmini süreyi hesapla:

{stopsInfo}

Görevler:
1. Duraklar arasındaki toplam yol mesafesini hesapla (Türkiye yollarını kullanarak)
2. Tahmini seyahat süresini hesapla (trafik ve mola dahil)
3. Yakıt maliyeti tahmini yap (ortalama dizel fiyatı: 35 TL/litre, ortalama tüketim: 30 lt/100km)
4. Geçiş ücreti tahmini yap (varsa)

SADECE JSON formatında yanıt ver:
{{
  ""totalDistanceKm"": toplam mesafe (sayısal),
  ""estimatedDurationMinutes"": tahmini süre dakika cinsinden (sayısal),
  ""estimatedFuelCost"": tahmini yakıt maliyeti TL (sayısal),
  ""estimatedTollCost"": tahmini geçiş ücreti TL (sayısal),
  ""routeDescription"": ""rota açıklaması""
}}";

                var response = await model.GenerateContentAsync(prompt);
                var resultText = response.Text.Trim();

                // Remove markdown code blocks if present
                if (resultText.StartsWith("```json"))
                {
                    resultText = resultText.Substring(7);
                }
                if (resultText.StartsWith("```"))
                {
                    resultText = resultText.Substring(3);
                }
                if (resultText.EndsWith("```"))
                {
                    resultText = resultText.Substring(0, resultText.Length - 3);
                }
                resultText = resultText.Trim();

                var result = JsonSerializer.Deserialize<RouteCalculationResult>(resultText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    result.Success = true;
                    _logger.LogInformation("AI calculated route: {Distance}km, {Duration}min",
                        result.TotalDistanceKm, result.EstimatedDurationMinutes);
                    return result;
                }

                _logger.LogWarning("Could not parse AI response for route calculation");
                return GetFallbackRouteCalculation(stops);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini AI for route calculation");
                return GetFallbackRouteCalculation(stops);
            }
        }

        private RouteCalculationResult GetFallbackRouteCalculation(List<RouteStopInfo> stops)
        {
            // Simple fallback: calculate straight-line distance and estimate
            decimal totalDistance = 0;

            for (int i = 0; i < stops.Count - 1; i++)
            {
                var distance = CalculateHaversineDistance(
                    stops[i].Latitude, stops[i].Longitude,
                    stops[i + 1].Latitude, stops[i + 1].Longitude);

                // Add 30% for road routing (not straight line)
                totalDistance += distance * 1.3m;
            }

            // Estimate duration (average speed 60 km/h)
            var estimatedDuration = (int)(totalDistance / 60m * 60m); // convert to minutes

            // Estimate fuel cost (30 liters per 100km, 35 TL per liter)
            var estimatedFuelCost = (totalDistance / 100m) * 30m * 35m;

            // Estimate toll cost (rough estimate based on distance)
            var estimatedTollCost = totalDistance > 200 ? totalDistance * 0.5m : 0;

            return new RouteCalculationResult
            {
                TotalDistanceKm = Math.Round(totalDistance, 2),
                EstimatedDurationMinutes = estimatedDuration,
                EstimatedFuelCost = Math.Round(estimatedFuelCost, 2),
                EstimatedTollCost = Math.Round(estimatedTollCost, 2),
                RouteDescription = "Otomatik hesaplama (basitleştirilmiş)",
                Success = true
            };
        }

        private decimal CalculateHaversineDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const decimal R = 6371; // Earth's radius in km

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = (decimal)(Math.Sin((double)dLat / 2) * Math.Sin((double)dLat / 2) +
                    Math.Cos((double)ToRadians(lat1)) * Math.Cos((double)ToRadians(lat2)) *
                    Math.Sin((double)dLon / 2) * Math.Sin((double)dLon / 2));

            var c = (decimal)(2 * Math.Atan2(Math.Sqrt((double)a), Math.Sqrt((double)(1 - a))));

            return R * c;
        }

        private decimal ToRadians(decimal degrees)
        {
            return degrees * (decimal)Math.PI / 180m;
        }
    }

    public class VehicleRecommendation
    {
        public string VehicleType { get; set; } = string.Empty;
        public int SuitabilityScore { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal MaxWeight { get; set; }
        public decimal? MaxVolume { get; set; }
        public decimal? EstimatedCost { get; set; }
    }

    public class RouteStopInfo
    {
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class RouteCalculationResult
    {
        public decimal TotalDistanceKm { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public decimal? EstimatedFuelCost { get; set; }
        public decimal? EstimatedTollCost { get; set; }
        public string? RouteDescription { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
