using AknaLoad.Domain.Dtos.Requests;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace aknaLoadMatchingApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                    PickupDateTime = request.PickupDateTime,
                    DeliveryDeadline = request.DeliveryDeadline,
                    FlexiblePickup = request.FlexiblePickup,
                    FlexibleDelivery = request.FlexibleDelivery,
                    Weight = request.Weight,
                    Volume = request.Volume,
                    Dimensions = request.Dimensions?.ToDimensions(),
                    LoadType = request.LoadType,
                    SpecialRequirements = request.SpecialRequirements ?? new List<SpecialRequirement>(),
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
        public async Task<ActionResult<Load>> UpdateLoad(long id, [FromBody] Load load)
        {
            try
            {
                if (id != load.Id)
                    return BadRequest(new { message = "ID mismatch" });

                var updatedBy = User?.Identity?.Name ?? "Anonymous";
                var updatedLoad = await _loadService.UpdateLoadAsync(load, updatedBy);

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
    }

    // DTOs for request bodies
    public class CancelLoadRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ExtendDeadlineRequest
    {
        public DateTime NewDeadline { get; set; }
    }
}