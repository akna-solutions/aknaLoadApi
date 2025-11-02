using AknaLoad.Domain.Dtos;
using AknaLoad.Domain.Entities;

namespace AknaLoad.Domain.Interfaces.Services
{
    /// <summary>
    /// Service for communicating with Identity Service to get Vehicle information
    /// </summary>
    public interface IVehicleService
    {
        Task<VehicleDto?> GetVehicleByIdAsync(long vehicleId);
        Task<List<VehicleDto>> GetVehiclesByDriverAsync(long driverId);
        Task<List<VehicleDto>> GetVehiclesByCompanyAsync(long companyId);
        Task<bool> IsVehicleAvailableAsync(long vehicleId);
        Task<bool> CanVehicleCarryLoadAsync(long vehicleId, Load load);
        Task<List<VehicleDto>> GetAvailableVehiclesAsync(DateTime fromDate, DateTime toDate);
    }
}