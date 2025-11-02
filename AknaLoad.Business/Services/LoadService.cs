using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Domain.Interfaces.Services;
using AknaLoad.Domain.Interfaces.UnitOfWorks;

namespace AknaLoad.Application.Services
{
    public class LoadService : ILoadService
    {
        private readonly ILoadRepository _loadRepository;
        private readonly IPricingService _pricingService;
        private readonly IUnitOfWork _unitOfWork;

        public LoadService(
            ILoadRepository loadRepository,
            IPricingService pricingService,
            IUnitOfWork unitOfWork)
        {
            _loadRepository = loadRepository;
            _pricingService = pricingService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Load> CreateLoadAsync(Load load, string createdBy)
        {
            await ValidateLoadAsync(load);

            load.LoadCode = await GenerateLoadCodeAsync();
            load.Status = LoadStatus.Draft;
            load.CreatedUser = createdBy;
            load.UpdatedUser = createdBy;

            await _loadRepository.AddAsync(load);
            await _unitOfWork.SaveChangesAsync();

            return load;
        }

        public async Task<Load> UpdateLoadAsync(Load load, string updatedBy)
        {
            await ValidateLoadAsync(load);

            load.UpdatedUser = updatedBy;
            _loadRepository.Update(load);
            await _unitOfWork.SaveChangesAsync();

            return load;
        }

        public async Task<bool> DeleteLoadAsync(long loadId, string deletedBy)
        {
            var result = await _loadRepository.SoftDeleteAsync(loadId);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }

        public async Task<Load?> GetLoadByIdAsync(long loadId)
        {
            return await _loadRepository.GetByIdAsync(loadId);
        }

        public async Task<Load?> GetLoadByCodeAsync(string loadCode)
        {
            return await _loadRepository.GetLoadByCodeAsync(loadCode);
        }

        public async Task<List<Load>> GetLoadsByOwnerAsync(long ownerId, LoadStatus? status = null)
        {
            return await _loadRepository.GetLoadsByOwnerAsync(ownerId, status);
        }

        public async Task<List<Load>> GetAvailableLoadsAsync(Location? driverLocation = null, int maxDistanceKm = 500)
        {
            return await _loadRepository.GetAvailableLoadsAsync(driverLocation, maxDistanceKm);
        }

        public async Task<bool> PublishLoadAsync(long loadId, string publishedBy)
        {
            var load = await _loadRepository.GetByIdAsync(loadId);
            if (load == null || load.Status != LoadStatus.Draft)
                return false;

            // Calculate price before publishing
            if (!load.FixedPrice.HasValue)
            {
                var price = await CalculateLoadPriceAsync(load);
                if (price.HasValue)
                {
                    load.FixedPrice = price.Value;
                }
            }

            await _loadRepository.UpdateLoadStatusAsync(loadId, LoadStatus.Published, publishedBy);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelLoadAsync(long loadId, string reason, string cancelledBy)
        {
            var load = await _loadRepository.GetByIdAsync(loadId);
            if (load == null)
                return false;

            // Only allow cancellation for certain statuses
            if (load.Status == LoadStatus.InTransit || load.Status == LoadStatus.Delivered)
                return false;

            await _loadRepository.UpdateLoadStatusAsync(loadId, LoadStatus.Cancelled, cancelledBy);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<decimal?> CalculateLoadPriceAsync(Load load)
        {
            try
            {
                var pricingCalculation = await _pricingService.CalculatePriceAsync(load);
                return pricingCalculation.CalculatedPrice;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Load>> SearchLoadsAsync(
            string? keyword = null,
            LoadType? loadType = null,
            LoadStatus? status = null,
            decimal? minWeight = null,
            decimal? maxWeight = null,
            string? pickupCity = null,
            string? deliveryCity = null,
            DateTime? pickupFromDate = null,
            DateTime? pickupToDate = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            return await _loadRepository.SearchLoadsAsync(
                keyword, loadType, status, minWeight, maxWeight,
                pickupCity, deliveryCity, pickupFromDate, pickupToDate,
                pageNumber, pageSize);
        }

        public async Task<string> GenerateLoadCodeAsync()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"LD{timestamp}{random}";
        }

        public async Task<bool> ValidateLoadAsync(Load load)
        {
            if (load.PickupLocation == null)
                throw new ArgumentException("Pickup location is required");

            if (load.DeliveryLocation == null)
                throw new ArgumentException("Delivery location is required");

            if (load.PickupDateTime >= load.DeliveryDeadline)
                throw new ArgumentException("Pickup date must be before delivery deadline");

            if (load.Weight <= 0)
                throw new ArgumentException("Weight must be greater than 0");

            if (string.IsNullOrEmpty(load.Title))
                throw new ArgumentException("Title is required");

            return true;
        }

        public async Task<List<Load>> GetExpiringLoadsAsync(int hoursAhead = 24)
        {
            var expiryDate = DateTime.UtcNow.AddHours(hoursAhead);
            return await _loadRepository.GetExpiringLoadsAsync(expiryDate);
        }

        public async Task<bool> ExtendDeadlineAsync(long loadId, DateTime newDeadline, string updatedBy)
        {
            var load = await _loadRepository.GetByIdAsync(loadId);
            if (load == null)
                return false;

            if (newDeadline <= load.DeliveryDeadline)
                return false;

            load.DeliveryDeadline = newDeadline;
            load.UpdatedUser = updatedBy;

            _loadRepository.Update(load);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}