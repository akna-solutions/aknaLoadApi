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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoadRepository _loadRepository;

        public LoadService(IUnitOfWork unitOfWork, ILoadRepository loadRepository)
        {
            _unitOfWork = unitOfWork;
            _loadRepository = loadRepository;
        }

        public async Task<Load> CreateLoadAsync(Load load, string createdBy)
        {
            // Validate load
            var validation = ValidateLoad(load);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Load validation failed: {string.Join(", ", validation.Errors)}");
            }

            // Generate unique load code
            load.LoadCode = await GenerateLoadCodeAsync();

            // Set metadata
            load.CreatedUser = createdBy;
            load.CreatedDate = DateTime.UtcNow;
            load.Status = LoadStatus.Draft;

            // Set multi-stop configuration
            load.IsMultiStop = load.LoadStops.Count > 2 ||
                              (load.LoadStops.Count == 2 && load.LoadStops.Any(s => s.StopType == LoadStopType.Both));
            load.TotalStops = load.LoadStops.Count;

            // Calculate route information if multi-stop
            if (load.IsMultiStop && load.LoadStops.Any())
            {
                CalculateRouteInformation(load);
            }

            // Add to repository
            await _loadRepository.AddAsync(load);
            await _unitOfWork.SaveChangesAsync();

            return load;
        }

        public async Task<Load?> GetLoadByIdAsync(long id)
        {
            return await _loadRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<Load?> GetLoadByCodeAsync(string loadCode)
        {
            return await _loadRepository.GetByLoadCodeAsync(loadCode);
        }

        public async Task<List<Load>> GetLoadsByCompanyIdAsync(
            long companyId,
            LoadStatus? status = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null)
        {
            return await _loadRepository.GetByCompanyIdAsync(companyId, status, createdFrom, createdTo);
        }

        public async Task<(List<Load> Items, int TotalCount)> GetPagedLoadsAsync(
            long? companyId = null,
            LoadStatus? status = null,
            List<LoadStatus>? statuses = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            DateTime? pickupFrom = null,
            DateTime? pickupTo = null,
            DateTime? deliveryFrom = null,
            DateTime? deliveryTo = null,
            LoadType? loadType = null,
            bool? isMultiStop = null,
            string? originCity = null,
            string? destinationCity = null,
            int pageNumber = 1,
            int pageSize = 20,
            string sortBy = "CreatedAt",
            bool sortDescending = true)
        {
            return await _loadRepository.GetPagedAsync(
                companyId, status, statuses, createdFrom, createdTo,
                pickupFrom, pickupTo, deliveryFrom, deliveryTo,
                loadType, isMultiStop, originCity, destinationCity,
                pageNumber, pageSize, sortBy, sortDescending);
        }

        public async Task<Load> UpdateLoadAsync(Load load, string updatedBy)
        {
            var existingLoad = await _loadRepository.GetByIdAsync(load.Id);
            if (existingLoad == null)
            {
                throw new ArgumentException($"Load with ID {load.Id} not found");
            }

            // Check if load can be updated
            if (existingLoad.Status == LoadStatus.Completed || existingLoad.Status == LoadStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot update load with status {existingLoad.Status}");
            }

            // Validate updated load
            var validation = ValidateLoad(load);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Load validation failed: {string.Join(", ", validation.Errors)}");
            }

            // Update metadata
            load.UpdatedUser = updatedBy;
            load.UpdatedDate = DateTime.UtcNow;

            // Recalculate route information if needed
            if (load.IsMultiStop && load.LoadStops.Any())
            {
                CalculateRouteInformation(load);
            }

            _loadRepository.Update(load);
            await _unitOfWork.SaveChangesAsync();

            return load;
        }

        public async Task<bool> DeleteLoadAsync(long id, string deletedBy)
        {
            var load = await _loadRepository.GetByIdAsync(id);
            if (load == null)
            {
                return false;
            }

            // Check if load can be deleted
            if (load.Status == LoadStatus.InTransit || load.Status == LoadStatus.PickedUp)
            {
                throw new InvalidOperationException($"Cannot delete load with status {load.Status}");
            }

            load.IsDeleted = true;
            load.UpdatedUser = deletedBy;
            load.UpdatedDate = DateTime.UtcNow;

            _loadRepository.Update(load);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<Load> PublishLoadAsync(long id, string publishedBy)
        {
            var load = await _loadRepository.GetByIdAsync(id);
            if (load == null)
            {
                throw new ArgumentException($"Load with ID {id} not found");
            }

            if (load.Status != LoadStatus.Draft)
            {
                throw new InvalidOperationException($"Only draft loads can be published. Current status: {load.Status}");
            }

            // Validate before publishing
            var validation = ValidateLoad(load);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Cannot publish invalid load: {string.Join(", ", validation.Errors)}");
            }

            load.Status = LoadStatus.Published;
            load.PublishedAt = DateTime.UtcNow;
            load.UpdatedUser = publishedBy;
            load.UpdatedDate = DateTime.UtcNow;

            _loadRepository.Update(load);
            await _unitOfWork.SaveChangesAsync();

            return load;
        }

        public async Task<Load> CancelLoadAsync(long id, string cancelledBy, string? reason = null)
        {
            var load = await _loadRepository.GetByIdAsync(id);
            if (load == null)
            {
                throw new ArgumentException($"Load with ID {id} not found");
            }

            if (load.Status == LoadStatus.Completed || load.Status == LoadStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot cancel load with status {load.Status}");
            }

            load.Status = LoadStatus.Cancelled;
            load.UpdatedUser = cancelledBy;
            load.UpdatedDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(reason))
            {
                load.Description = $"{load.Description}\n\nCancellation reason: {reason}";
            }

            _loadRepository.Update(load);
            await _unitOfWork.SaveChangesAsync();

            return load;
        }

        public (bool IsValid, List<string> Errors) ValidateLoad(Load load)
        {
            var errors = new List<string>();

            // Basic validation
            if (load.CompanyId <= 0)
                errors.Add("CompanyId is required");

            if (string.IsNullOrWhiteSpace(load.Title))
                errors.Add("Title is required");

            if (load.Weight <= 0)
                errors.Add("Weight must be greater than 0");

            // LoadStops validation
            if (!load.LoadStops.Any())
            {
                errors.Add("At least one load stop is required");
            }
            else
            {
                // Check stop order
                var stopOrders = load.LoadStops.Select(s => s.StopOrder).OrderBy(o => o).ToList();
                for (int i = 0; i < stopOrders.Count; i++)
                {
                    if (stopOrders[i] != i + 1)
                    {
                        errors.Add($"Invalid stop order. Expected {i + 1}, found {stopOrders[i]}");
                        break;
                    }
                }

                // Validate each stop
                foreach (var stop in load.LoadStops)
                {
                    var stopValidation = stop.ValidateStop();
                    if (!stopValidation.IsValid)
                    {
                        errors.AddRange(stopValidation.Errors.Select(e => $"Stop {stop.StopOrder}: {e}"));
                    }
                }

                // Check pickup and delivery balance
                var totalPickupWeight = load.LoadStops.Sum(s => s.PickupWeight ?? 0);
                var totalDeliveryWeight = load.LoadStops.Sum(s => s.DeliveryWeight ?? 0);

                if (Math.Abs(totalPickupWeight - totalDeliveryWeight) > 0.01m)
                {
                    errors.Add($"Total pickup weight ({totalPickupWeight}kg) must equal total delivery weight ({totalDeliveryWeight}kg)");
                }
            }

            return (errors.Count == 0, errors);
        }

        private async Task<string> GenerateLoadCodeAsync()
        {
            string loadCode;
            bool isUnique;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                // Format: LOAD-YYYYMMDD-XXXXX (e.g., LOAD-20241114-A1B2C)
                var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
                var randomPart = GenerateRandomString(5);
                loadCode = $"LOAD-{datePart}-{randomPart}";

                isUnique = await _loadRepository.IsLoadCodeUniqueAsync(loadCode);
                attempts++;

            } while (!isUnique && attempts < maxAttempts);

            if (!isUnique)
            {
                throw new InvalidOperationException("Failed to generate unique load code");
            }

            return loadCode;
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude similar looking characters
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        private void CalculateRouteInformation(Load load)
        {
            if (!load.LoadStops.Any()) return;

            var orderedStops = load.LoadStops.OrderBy(s => s.StopOrder).ToList();

            // Calculate total distance (simple approximation using Location.DistanceTo)
            double totalDistance = 0;
            for (int i = 0; i < orderedStops.Count - 1; i++)
            {
                var currentLocation = orderedStops[i].Location;
                var nextLocation = orderedStops[i + 1].Location;

                if (currentLocation != null && nextLocation != null)
                {
                    totalDistance += currentLocation.DistanceTo(nextLocation);
                }
            }

            load.TotalDistanceKm = (decimal)totalDistance;

            // Calculate total duration
            var totalDuration = orderedStops.Sum(s => s.EstimatedDurationMinutes);
            // Add driving time estimate (rough: 1 hour per 60km)
            totalDuration += (int)(totalDistance);
            load.EstimatedTotalDurationMinutes = totalDuration;

            // Set earliest pickup and latest delivery
            var pickupStops = orderedStops
                .Where(s => s.IsPickupStop && s.EarliestTime.HasValue)
                .ToList();
            load.EarliestPickupTime = pickupStops.Any()
                ? pickupStops.Min(s => s.EarliestTime)
                : null;

            var deliveryStops = orderedStops
                .Where(s => s.IsDeliveryStop && s.LatestTime.HasValue)
                .ToList();
            load.LatestDeliveryTime = deliveryStops.Any()
                ? deliveryStops.Max(s => s.LatestTime)
                : null;
        }
    }
}