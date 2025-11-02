using AknaLoad.Domain.Dtos.Requests;
using AknaLoad.Domain.Dtos;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using AknaLoad.Domain.Entities.ValueObjects;

namespace AknaLoad.API.Controllers
{
    [ApiController]
    [Route("api/loads")]
    public class LoadController : ControllerBase
    {
        private readonly ILoadService _loadService;
        private readonly IPricingService _pricingService;

        public LoadController(ILoadService loadService, IPricingService pricingService)
        {
            _loadService = loadService;
            _pricingService = pricingService;
        }

        /// <summary>
        /// Get all loads for a specific owner
        /// </summary>
        [HttpGet("owner/{ownerId}")]
        public async Task<ActionResult<List<Load>>> GetLoadsByOwner(long ownerId, [FromQuery] LoadStatus? status = null)
        {
            try
            {
                var loads = await _loadService.GetLoadsByOwnerAsync(ownerId, status);
                return Ok(loads);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get available loads for drivers
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<List<Load>>> GetAvailableLoads(
            [FromQuery] decimal? driverLat = null,
            [FromQuery] decimal? driverLng = null,
            [FromQuery] int maxDistanceKm = 500)
        {
            try
            {
                Location? driverLocation = null;
                if (driverLat.HasValue && driverLng.HasValue)
                {
                    driverLocation = new Location(driverLat.Value, driverLng.Value, "", "");
                }

                var loads = await _loadService.GetAvailableLoadsAsync(driverLocation, maxDistanceKm);
                return Ok(loads);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get load by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Load>> GetLoadById(long id)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                return Ok(load);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get load by code
        /// </summary>
        [HttpGet("code/{loadCode}")]
        public async Task<ActionResult<Load>> GetLoadByCode(string loadCode)
        {
            try
            {
                var load = await _loadService.GetLoadByCodeAsync(loadCode);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                return Ok(load);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new load
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Load>> CreateLoad([FromBody] CreateLoadRequest request)
        {
            try
            {
                var createdBy = User?.Identity?.Name ?? "Anonymous";

                // DTO'dan Entity'ye çevir
                var load = new Load
                {
                    OwnerId = request.OwnerId,
                    Title = request.Title,
                    Description = request.Description,
                    PickupLocation = request.PickupLocation.ToLocation(),
                    DeliveryLocation = request.DeliveryLocation.ToLocation(),
                    PickupDateTime = request.PickupDateTime ?? DateTime.MinValue,
                    DeliveryDeadline = request.DeliveryDeadline ?? DateTime.MinValue,
                    FlexiblePickup = request.FlexiblePickup,
                    FlexibleDelivery = request.FlexibleDelivery,
                    Weight = request.Weight,
                    Volume = request.Volume,
                    Dimensions = request.Dimensions?.ToDimensions(),
                    LoadType = request.GetLoadType(),
                    SpecialRequirements = request.GetSpecialRequirements(),
                    DistanceKm = request.DistanceKm,
                    EstimatedDurationMinutes = request.EstimatedDurationMinutes,
                    PickupInstructions = request.PickupInstructions,
                    DeliveryInstructions = request.DeliveryInstructions,
                    ContactPersonName = request.ContactPersonName,
                    ContactPhone = request.ContactPhone,
                    ContactEmail = request.ContactEmail
                };

                var createdLoad = await _loadService.CreateLoadAsync(load, createdBy);

                return CreatedAtAction(nameof(GetLoadById), new { id = createdLoad.Id }, createdLoad);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing load
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Load>> UpdateLoad(long id, [FromBody] CreateLoadRequest request)
        {
            try
            {
                var existingLoad = await _loadService.GetLoadByIdAsync(id);
                if (existingLoad == null)
                    return NotFound(new { message = "Load not found" });

                var updatedBy = User?.Identity?.Name ?? "Anonymous";

                // Update the existing load with new data
                existingLoad.Title = request.Title;
                existingLoad.Description = request.Description;
                existingLoad.PickupLocation = request.PickupLocation.ToLocation();
                existingLoad.DeliveryLocation = request.DeliveryLocation.ToLocation();
                existingLoad.PickupDateTime = request.PickupDateTime ?? DateTime.MinValue;
                existingLoad.DeliveryDeadline = request.DeliveryDeadline ?? DateTime.MinValue;
                existingLoad.FlexiblePickup = request.FlexiblePickup;
                existingLoad.FlexibleDelivery = request.FlexibleDelivery;
                existingLoad.Weight = request.Weight;
                existingLoad.Volume = request.Volume;
                existingLoad.Dimensions = request.Dimensions?.ToDimensions();
                existingLoad.LoadType = request.GetLoadType();
                existingLoad.SpecialRequirements = request.GetSpecialRequirements();
                existingLoad.DistanceKm = request.DistanceKm;
                existingLoad.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
                existingLoad.PickupInstructions = request.PickupInstructions;
                existingLoad.DeliveryInstructions = request.DeliveryInstructions;
                existingLoad.ContactPersonName = request.ContactPersonName;
                existingLoad.ContactPhone = request.ContactPhone;
                existingLoad.ContactEmail = request.ContactEmail;

                var updatedLoad = await _loadService.UpdateLoadAsync(existingLoad, updatedBy);

                return Ok(updatedLoad);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a load
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLoad(long id)
        {
            try
            {
                var deletedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _loadService.DeleteLoadAsync(id, deletedBy);

                if (!result)
                    return NotFound(new { message = "Load not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Publish a load to make it available for matching
        /// </summary>
        [HttpPost("{id}/publish")]
        public async Task<ActionResult> PublishLoad(long id)
        {
            try
            {
                var publishedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _loadService.PublishLoadAsync(id, publishedBy);

                if (!result)
                    return BadRequest(new { message = "Cannot publish load. Check load status and pricing." });

                return Ok(new { message = "Load published successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a load
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelLoad(long id, [FromBody] CancelLoadRequest request)
        {
            try
            {
                var cancelledBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _loadService.CancelLoadAsync(id, request.Reason, cancelledBy);

                if (!result)
                    return BadRequest(new { message = "Cannot cancel load. Check load status." });

                return Ok(new { message = "Load cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Calculate price for a load
        /// </summary>
        [HttpPost("{id}/calculate-price")]
        public async Task<ActionResult<decimal>> CalculateLoadPrice(long id)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                var price = await _loadService.CalculateLoadPriceAsync(load);
                if (!price.HasValue)
                    return BadRequest(new { message = "Could not calculate price for this load" });

                return Ok(new { calculatedPrice = price.Value });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get pricing calculation details for a load
        /// </summary>
        [HttpGet("{id}/pricing")]
        public async Task<ActionResult> GetLoadPricing(long id)
        {
            try
            {
                var calculation = await _pricingService.GetLatestCalculationAsync(id);
                if (calculation == null)
                    return NotFound(new { message = "No pricing calculation found" });

                return Ok(calculation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search loads with filters
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<Load>>> SearchLoads(
            [FromQuery] string? keyword = null,
            [FromQuery] LoadType? loadType = null,
            [FromQuery] LoadStatus? status = null,
            [FromQuery] decimal? minWeight = null,
            [FromQuery] decimal? maxWeight = null,
            [FromQuery] string? pickupCity = null,
            [FromQuery] string? deliveryCity = null,
            [FromQuery] DateTime? pickupFromDate = null,
            [FromQuery] DateTime? pickupToDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var loads = await _loadService.SearchLoadsAsync(
                    keyword, loadType, status, minWeight, maxWeight,
                    pickupCity, deliveryCity, pickupFromDate, pickupToDate,
                    pageNumber, pageSize);

                return Ok(loads);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get loads expiring soon
        /// </summary>
        [HttpGet("expiring")]
        public async Task<ActionResult<List<Load>>> GetExpiringLoads([FromQuery] int hoursAhead = 24)
        {
            try
            {
                var loads = await _loadService.GetExpiringLoadsAsync(hoursAhead);
                return Ok(loads);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Extend delivery deadline for a load
        /// </summary>
        [HttpPost("{id}/extend-deadline")]
        public async Task<ActionResult> ExtendDeadline(long id, [FromBody] ExtendDeadlineRequest request)
        {
            try
            {
                var updatedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _loadService.ExtendDeadlineAsync(id, request.NewDeadline, updatedBy);

                if (!result)
                    return BadRequest(new { message = "Cannot extend deadline. Check load status and new deadline." });

                return Ok(new { message = "Deadline extended successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #region Multi-Stop Load Endpoints

        /// <summary>
        /// Create a new multi-stop load
        /// </summary>
        [HttpPost("multi-stop")]
        public async Task<ActionResult<MultiStopLoadResponseDto>> CreateMultiStopLoad([FromBody] CreateMultiStopLoadRequest request)
        {
            try
            {
                var createdBy = User?.Identity?.Name ?? "Anonymous";

                // Create the main load
                var load = new Load
                {
                    OwnerId = request.OwnerId,
                    Title = request.Title,
                    Description = request.Description,
                    Weight = request.Weight,
                    Volume = request.Volume,
                    Dimensions = request.Dimensions?.ToDimensions(),
                    LoadType = request.GetLoadType(),
                    SpecialRequirements = request.GetSpecialRequirements(),
                    ContactPersonName = request.ContactPersonName,
                    ContactPhone = request.ContactPhone,
                    ContactEmail = request.ContactEmail,
                    IsMultiStop = true,
                    RoutingStrategy = request.GetRoutingStrategy(),
                    TotalStops = request.LoadStops.Count
                };

                // Calculate time window
                if (request.LoadStops.Any())
                {
                    var earliestTimes = request.LoadStops.Where(s => s.EarliestTime.HasValue).Select(s => s.EarliestTime!.Value);
                    var latestTimes = request.LoadStops.Where(s => s.LatestTime.HasValue).Select(s => s.LatestTime!.Value);

                    if (earliestTimes.Any())
                        load.EarliestPickupTime = earliestTimes.Min();
                    if (latestTimes.Any())
                        load.LatestDeliveryTime = latestTimes.Max();
                }

                var createdLoad = await _loadService.CreateLoadAsync(load, createdBy);

                // Create load stops
                foreach (var stopDto in request.LoadStops)
                {
                    var loadStop = new LoadStop
                    {
                        LoadId = createdLoad.Id,
                        StopOrder = stopDto.StopOrder,
                        StopType = stopDto.GetStopType(),
                        Location = stopDto.Location.ToLocation(),
                        EarliestTime = stopDto.EarliestTime,
                        LatestTime = stopDto.LatestTime,
                        PlannedTime = stopDto.PlannedTime,
                        EstimatedDurationMinutes = stopDto.EstimatedDurationMinutes,
                        PickupWeight = stopDto.PickupWeight,
                        DeliveryWeight = stopDto.DeliveryWeight,
                        PickupVolume = stopDto.PickupVolume,
                        DeliveryVolume = stopDto.DeliveryVolume,
                        LoadDescription = stopDto.LoadDescription,
                        SpecialInstructions = stopDto.SpecialInstructions,
                        SpecialRequirements = stopDto.GetSpecialRequirements(),
                        ContactPersonName = stopDto.ContactPersonName,
                        ContactPhone = stopDto.ContactPhone,
                        ContactEmail = stopDto.ContactEmail,
                        CreatedUser = createdBy,
                        UpdatedUser = createdBy
                    };

                    createdLoad.LoadStops.Add(loadStop);
                }

                await _loadService.UpdateLoadAsync(createdLoad, createdBy);

                // Convert to response DTO
                var response = ConvertToMultiStopResponse(createdLoad);

                return CreatedAtAction(nameof(GetMultiStopLoad), new { id = createdLoad.Id }, response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Get multi-stop load with all stops
        /// </summary>
        [HttpGet("{id}/multi-stop")]
        public async Task<ActionResult<MultiStopLoadResponseDto>> GetMultiStopLoad(long id)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                if (!load.IsMultiStop)
                    return BadRequest(new { message = "This is not a multi-stop load" });

                var response = ConvertToMultiStopResponse(load);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add a new stop to existing multi-stop load
        /// </summary>
        [HttpPost("{id}/stops")]
        public async Task<ActionResult<LoadStopResponseDto>> AddLoadStop(long id, [FromBody] LoadStopDto stopDto)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                if (!load.IsMultiStop)
                    return BadRequest(new { message = "Cannot add stops to single-stop load" });

                if (load.Status != LoadStatus.Draft)
                    return BadRequest(new { message = "Cannot modify published load" });

                var createdBy = User?.Identity?.Name ?? "Anonymous";

                var loadStop = new LoadStop
                {
                    LoadId = id,
                    StopOrder = stopDto.StopOrder,
                    StopType = stopDto.GetStopType(),
                    Location = stopDto.Location.ToLocation(),
                    EarliestTime = stopDto.EarliestTime,
                    LatestTime = stopDto.LatestTime,
                    PlannedTime = stopDto.PlannedTime,
                    EstimatedDurationMinutes = stopDto.EstimatedDurationMinutes,
                    PickupWeight = stopDto.PickupWeight,
                    DeliveryWeight = stopDto.DeliveryWeight,
                    PickupVolume = stopDto.PickupVolume,
                    DeliveryVolume = stopDto.DeliveryVolume,
                    LoadDescription = stopDto.LoadDescription,
                    SpecialInstructions = stopDto.SpecialInstructions,
                    SpecialRequirements = stopDto.GetSpecialRequirements(),
                    ContactPersonName = stopDto.ContactPersonName,
                    ContactPhone = stopDto.ContactPhone,
                    ContactEmail = stopDto.ContactEmail,
                    CreatedUser = createdBy,
                    UpdatedUser = createdBy
                };

                load.LoadStops.Add(loadStop);
                load.TotalStops = load.LoadStops.Count;

                await _loadService.UpdateLoadAsync(load, createdBy);

                var response = ConvertToStopResponse(loadStop);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a specific load stop
        /// </summary>
        [HttpPut("{loadId}/stops/{stopId}")]
        public async Task<ActionResult<LoadStopResponseDto>> UpdateLoadStop(long loadId, long stopId, [FromBody] LoadStopDto stopDto)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(loadId);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                var stop = load.LoadStops.FirstOrDefault(s => s.Id == stopId);
                if (stop == null)
                    return NotFound(new { message = "Stop not found" });

                if (load.Status != LoadStatus.Draft)
                    return BadRequest(new { message = "Cannot modify published load" });

                var updatedBy = User?.Identity?.Name ?? "Anonymous";

                // Update stop properties
                stop.StopOrder = stopDto.StopOrder;
                stop.StopType = stopDto.GetStopType();
                stop.Location = stopDto.Location.ToLocation();
                stop.EarliestTime = stopDto.EarliestTime;
                stop.LatestTime = stopDto.LatestTime;
                stop.PlannedTime = stopDto.PlannedTime;
                stop.EstimatedDurationMinutes = stopDto.EstimatedDurationMinutes;
                stop.PickupWeight = stopDto.PickupWeight;
                stop.DeliveryWeight = stopDto.DeliveryWeight;
                stop.PickupVolume = stopDto.PickupVolume;
                stop.DeliveryVolume = stopDto.DeliveryVolume;
                stop.LoadDescription = stopDto.LoadDescription;
                stop.SpecialInstructions = stopDto.SpecialInstructions;
                stop.SpecialRequirements = stopDto.GetSpecialRequirements();
                stop.ContactPersonName = stopDto.ContactPersonName;
                stop.ContactPhone = stopDto.ContactPhone;
                stop.ContactEmail = stopDto.ContactEmail;
                stop.UpdatedUser = updatedBy;

                await _loadService.UpdateLoadAsync(load, updatedBy);

                var response = ConvertToStopResponse(stop);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a load stop
        /// </summary>
        [HttpDelete("{loadId}/stops/{stopId}")]
        public async Task<ActionResult> DeleteLoadStop(long loadId, long stopId)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(loadId);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                var stop = load.LoadStops.FirstOrDefault(s => s.Id == stopId);
                if (stop == null)
                    return NotFound(new { message = "Stop not found" });

                if (load.Status != LoadStatus.Draft)
                    return BadRequest(new { message = "Cannot modify published load" });

                var updatedBy = User?.Identity?.Name ?? "Anonymous";

                load.LoadStops.Remove(stop);
                load.TotalStops = load.LoadStops.Count;

                await _loadService.UpdateLoadAsync(load, updatedBy);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Convert single-stop load to multi-stop
        /// </summary>
        [HttpPost("{id}/convert-to-multistop")]
        public async Task<ActionResult<MultiStopLoadResponseDto>> ConvertToMultiStop(long id)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                if (load.IsMultiStop)
                    return BadRequest(new { message = "Load is already multi-stop" });

                if (load.Status != LoadStatus.Draft)
                    return BadRequest(new { message = "Cannot convert published load" });

                var updatedBy = User?.Identity?.Name ?? "Anonymous";

                load.ConvertToMultiStop();
                await _loadService.UpdateLoadAsync(load, updatedBy);

                var response = ConvertToMultiStopResponse(load);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Optimize route for multi-stop load
        /// </summary>
        [HttpPost("{id}/optimize-route")]
        public async Task<ActionResult<MultiStopLoadResponseDto>> OptimizeRoute(long id, [FromBody] OptimizeRouteRequest request)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id);
                if (load == null)
                    return NotFound(new { message = "Load not found" });

                if (!load.IsMultiStop)
                    return BadRequest(new { message = "Route optimization only available for multi-stop loads" });

                // TODO: Implement route optimization algorithm
                // For now, just return current load
                var response = ConvertToMultiStopResponse(load);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Helper Methods

        private MultiStopLoadResponseDto ConvertToMultiStopResponse(Load load)
        {
            return new MultiStopLoadResponseDto
            {
                Id = load.Id,
                LoadCode = load.LoadCode,
                Title = load.Title,
                Description = load.Description,
                Status = load.Status.ToString(),
                IsMultiStop = load.IsMultiStop,
                RoutingStrategy = load.RoutingStrategy.ToString(),
                TotalStops = load.TotalStops,
                Weight = load.Weight,
                Volume = load.Volume,
                LoadType = load.LoadType.ToString(),
                SpecialRequirements = load.SpecialRequirements.Select(r => r.ToString()).ToList(),
                TotalDistanceKm = load.TotalDistanceKm,
                EstimatedTotalDurationMinutes = load.EstimatedTotalDurationMinutes,
                EarliestPickupTime = load.EarliestPickupTime,
                LatestDeliveryTime = load.LatestDeliveryTime,
                FixedPrice = load.FixedPrice,
                ContactPersonName = load.ContactPersonName,
                ContactPhone = load.ContactPhone,
                ContactEmail = load.ContactEmail,
                LoadStops = load.LoadStops.OrderBy(s => s.StopOrder).Select(ConvertToStopResponse).ToList(),
                CreatedDate = load.CreatedDate,
                PublishedAt = load.PublishedAt,
                MatchedAt = load.MatchedAt
            };
        }

        private LoadStopResponseDto ConvertToStopResponse(LoadStop stop)
        {
            return new LoadStopResponseDto
            {
                Id = stop.Id,
                StopOrder = stop.StopOrder,
                StopType = stop.StopType.ToString(),
                Status = stop.Status.ToString(),
                Location = ConvertLocationToDto(stop.Location!),
                EarliestTime = stop.EarliestTime,
                LatestTime = stop.LatestTime,
                PlannedTime = stop.PlannedTime,
                EstimatedDurationMinutes = stop.EstimatedDurationMinutes,
                PickupWeight = stop.PickupWeight,
                DeliveryWeight = stop.DeliveryWeight,
                PickupVolume = stop.PickupVolume,
                DeliveryVolume = stop.DeliveryVolume,
                LoadDescription = stop.LoadDescription,
                SpecialInstructions = stop.SpecialInstructions,
                SpecialRequirements = stop.SpecialRequirements.Select(r => r.ToString()).ToList(),
                ContactPersonName = stop.ContactPersonName,
                ContactPhone = stop.ContactPhone,
                ContactEmail = stop.ContactEmail,
                ActualArrivalTime = stop.ActualArrivalTime,
                ActualDepartureTime = stop.ActualDepartureTime,
                CompletionNotes = stop.CompletionNotes,
                HasSignature = !string.IsNullOrEmpty(stop.SignatureUrl),
                PhotoUrls = stop.PhotoUrls
            };
        }

        private LocationDto ConvertLocationToDto(Location location)
        {
            return new LocationDto
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Address = location.Address,
                City = location.City,
                District = location.District,
                PostalCode = location.PostalCode,
                Country = location.Country,
                LocationName = location.LocationName,
                AccessInstructions = location.AccessInstructions,
                ContactPerson = location.ContactPerson,
                ContactPhone = location.ContactPhone
            };
        }

        #endregion
    }

}