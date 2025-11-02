using AknaLoad.Domain.Dtos;
using AknaLoad.Domain.Dtos.Responses;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AknaLoad.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VehicleService> _logger;
        private readonly string _identityServiceBaseUrl;

        public VehicleService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<VehicleService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _identityServiceBaseUrl = _configuration["Services:IdentityService:BaseUrl"] ?? "https://localhost:7001";
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(long vehicleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_identityServiceBaseUrl}/api/vehicle/{vehicleId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<VehicleDto>(jsonContent, GetJsonOptions());
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }

                _logger.LogWarning("Failed to get vehicle {VehicleId}. Status: {StatusCode}",
                    vehicleId, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle {VehicleId}", vehicleId);
                return null;
            }
        }

        public async Task<List<VehicleDto>> GetVehiclesByDriverAsync(long driverId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_identityServiceBaseUrl}/api/vehicle/driver/{driverId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<VehicleDto>>(jsonContent, GetJsonOptions()) ?? new List<VehicleDto>();
                }

                _logger.LogWarning("Failed to get vehicles for driver {DriverId}. Status: {StatusCode}",
                    driverId, response.StatusCode);
                return new List<VehicleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles for driver {DriverId}", driverId);
                return new List<VehicleDto>();
            }
        }

        public async Task<List<VehicleDto>> GetVehiclesByCompanyAsync(long companyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_identityServiceBaseUrl}/api/vehicle/company/{companyId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<VehicleDto>>(jsonContent, GetJsonOptions()) ?? new List<VehicleDto>();
                }

                _logger.LogWarning("Failed to get vehicles for company {CompanyId}. Status: {StatusCode}",
                    companyId, response.StatusCode);
                return new List<VehicleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles for company {CompanyId}", companyId);
                return new List<VehicleDto>();
            }
        }

        public async Task<bool> IsVehicleAvailableAsync(long vehicleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_identityServiceBaseUrl}/api/vehicle/{vehicleId}/availability");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<VehicleAvailabilityResponse>(jsonContent, GetJsonOptions());
                    return result?.IsAvailable ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking vehicle availability {VehicleId}", vehicleId);
                return false;
            }
        }

        public async Task<bool> CanVehicleCarryLoadAsync(long vehicleId, Load load)
        {
            var vehicle = await GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
                return false;

            // Check weight capacity
            if (!vehicle.CanCarryWeight(load.Weight))
                return false;

            // Check volume capacity
            if (load.Volume.HasValue && !vehicle.CanCarryVolume(load.Volume.Value))
                return false;

            // Check dimensions if available
            if (load.Dimensions != null)
            {
                if (!vehicle.CanCarryDimensions(load.Dimensions.Length, load.Dimensions.Width, load.Dimensions.Height))
                    return false;
            }

            // Check special requirements
            if (load.SpecialRequirements != null && load.SpecialRequirements.Any())
            {
                foreach (var requirement in load.SpecialRequirements)
                {
                    switch (requirement)
                    {
                        case SpecialRequirement.Refrigerated:
                        case SpecialRequirement.ColdChain:
                        case SpecialRequirement.TemperatureControlled:
                            if (!vehicle.IsRefrigerated)
                                return false;
                            break;

                        case SpecialRequirement.Hazardous:
                            if (!vehicle.HazmatAllowed)
                                return false;
                            break;

                        case SpecialRequirement.Container:
                            if (!vehicle.CanCarryContainer)
                                return false;
                            break;
                    }
                }
            }

            return true;
        }

        public async Task<List<VehicleDto>> GetAvailableVehiclesAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_identityServiceBaseUrl}/api/vehicle/available?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<VehicleDto>>(jsonContent, GetJsonOptions()) ?? new List<VehicleDto>();
                }

                _logger.LogWarning("Failed to get available vehicles. Status: {StatusCode}", response.StatusCode);
                return new List<VehicleDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available vehicles");
                return new List<VehicleDto>();
            }
        }

        private static JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

   
    }
}