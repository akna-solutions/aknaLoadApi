using AknaLoad.Api.DTOs.Load;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Entities.ValueObjects;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AknaLoad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoadsController : ControllerBase
    {
        private readonly ILoadService _loadService;
        private readonly ILogger<LoadsController> _logger;

        public LoadsController(ILoadService loadService, ILogger<LoadsController> logger)
        {
            _loadService = loadService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new load (single or multi-stop)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(LoadResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLoad([FromBody] CreateLoadDto dto)
        {
            try
            {
                var load = MapToEntity(dto);
                var createdBy = "system"; // TODO: Get from authentication context

                var createdLoad = await _loadService.CreateLoadAsync(load, createdBy);

                var response = MapToResponseDto(createdLoad);
                return CreatedAtAction(nameof(GetLoadById), new { id = createdLoad.Id }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating load");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating load");
                return StatusCode(500, new { error = "An error occurred while creating the load" });
            }
        }

        /// <summary>
        /// Get load by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LoadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLoadById(long id)
        {
            try
            {
                var load = await _loadService.GetLoadByIdAsync(id);
                if (load == null)
                {
                    return NotFound(new { error = $"Load with ID {id} not found" });
                }

                var response = MapToResponseDto(load);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving load {LoadId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the load" });
            }
        }

        /// <summary>
        /// Get load by load code
        /// </summary>
        [HttpGet("code/{loadCode}")]
        [ProducesResponseType(typeof(LoadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLoadByCode(string loadCode)
        {
            try
            {
                var load = await _loadService.GetLoadByCodeAsync(loadCode);
                if (load == null)
                {
                    return NotFound(new { error = $"Load with code {loadCode} not found" });
                }

                var response = MapToResponseDto(load);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving load by code {LoadCode}", loadCode);
                return StatusCode(500, new { error = "An error occurred while retrieving the load" });
            }
        }

        /// <summary>
        /// Get paged loads with filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<LoadListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoads([FromQuery] LoadFilterDto filter)
        {
            try
            {
                var (items, totalCount) = await _loadService.GetPagedLoadsAsync(
                    filter.CompanyId,
                    filter.Status,
                    filter.Statuses,
                    filter.CreatedFrom,
                    filter.CreatedTo,
                    filter.PickupFrom,
                    filter.PickupTo,
                    filter.DeliveryFrom,
                    filter.DeliveryTo,
                    filter.LoadType,
                    filter.IsMultiStop,
                    filter.OriginCity,
                    filter.DestinationCity,
                    filter.PageNumber,
                    filter.PageSize,
                    filter.SortBy,
                    filter.SortDescending);

                var response = new PagedResultDto<LoadListItemDto>
                {
                    Items = items.Select(MapToListItemDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loads");
                return StatusCode(500, new { error = "An error occurred while retrieving loads" });
            }
        }

        /// <summary>
        /// Get loads by company ID
        /// </summary>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(typeof(List<LoadListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLoadsByCompanyId(
            long companyId,
            [FromQuery] LoadStatus? status = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null)
        {
            try
            {
                var loads = await _loadService.GetLoadsByCompanyIdAsync(companyId, status, createdFrom, createdTo);
                var response = loads.Select(MapToListItemDto).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loads for company {CompanyId}", companyId);
                return StatusCode(500, new { error = "An error occurred while retrieving loads" });
            }
        }

        /// <summary>
        /// Publish load (make it available for matching)
        /// </summary>
        [HttpPost("{id}/publish")]
        [ProducesResponseType(typeof(LoadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PublishLoad(long id)
        {
            try
            {
                var publishedBy = "system"; // TODO: Get from authentication context
                var load = await _loadService.PublishLoadAsync(id, publishedBy);
                var response = MapToResponseDto(load);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error publishing load {LoadId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while publishing load {LoadId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing load {LoadId}", id);
                return StatusCode(500, new { error = "An error occurred while publishing the load" });
            }
        }

        /// <summary>
        /// Cancel load
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(LoadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelLoad(long id, [FromBody] CancelLoadDto dto)
        {
            try
            {
                var cancelledBy = "system"; // TODO: Get from authentication context
                var load = await _loadService.CancelLoadAsync(id, cancelledBy, dto.Reason);
                var response = MapToResponseDto(load);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error cancelling load {LoadId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while cancelling load {LoadId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling load {LoadId}", id);
                return StatusCode(500, new { error = "An error occurred while cancelling the load" });
            }
        }

        /// <summary>
        /// Delete load
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteLoad(long id)
        {
            try
            {
                var deletedBy = "system"; // TODO: Get from authentication context
                var result = await _loadService.DeleteLoadAsync(id, deletedBy);
                if (!result)
                {
                    return NotFound(new { error = $"Load with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while deleting load {LoadId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting load {LoadId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the load" });
            }
        }

        #region Mapping Methods

        private Load MapToEntity(CreateLoadDto dto)
        {
            var load = new Load
            {
                CompanyId = dto.CompanyId,
                Title = dto.Title,
                Description = dto.Description,
                Weight = dto.Weight,
                Volume = dto.Volume,
                LoadType = dto.LoadType,
                FixedPrice = dto.FixedPrice,
                ContactPersonName = dto.ContactPersonName,
                ContactPhone = dto.ContactPhone,
                ContactEmail = dto.ContactEmail,
                IsMultiStop = dto.IsMultiStop,
                RoutingStrategy = dto.RoutingStrategy,
                SpecialRequirements = dto.SpecialRequirements ?? new List<SpecialRequirement>(),
                Dimensions = dto.Dimensions != null ? new Dimensions
                {
                    Length = dto.Dimensions.Length,
                    Width = dto.Dimensions.Width,
                    Height = dto.Dimensions.Height,
                    Unit = dto.Dimensions.Unit
                } : null
            };

            // Map load stops
            foreach (var stopDto in dto.LoadStops)
            {
                load.LoadStops.Add(new LoadStop
                {
                    StopOrder = stopDto.StopOrder,
                    StopType = stopDto.StopType,
                    Location = new Location
                    {
                        Latitude = stopDto.Location.Latitude,
                        Longitude = stopDto.Location.Longitude,
                        Address = stopDto.Location.Address,
                        City = stopDto.Location.City,
                        District = stopDto.Location.District,
                        PostalCode = stopDto.Location.PostalCode,
                        Country = stopDto.Location.Country,
                        LocationName = stopDto.Location.LocationName,
                        AccessInstructions = stopDto.Location.AccessInstructions,
                        ContactPerson = stopDto.Location.ContactPerson,
                        ContactPhone = stopDto.Location.ContactPhone
                    },
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
                    SpecialRequirements = stopDto.SpecialRequirements ?? new List<SpecialRequirement>(),
                    ContactPersonName = stopDto.ContactPersonName,
                    ContactPhone = stopDto.ContactPhone,
                    ContactEmail = stopDto.ContactEmail
                });
            }

            return load;
        }

        private LoadResponseDto MapToResponseDto(Load load)
        {
            return new LoadResponseDto
            {
                Id = load.Id,
                CompanyId = load.CompanyId,
                Title = load.Title,
                Description = load.Description,
                Status = load.Status,
                LoadCode = load.LoadCode,
                IsMultiStop = load.IsMultiStop,
                RoutingStrategy = load.RoutingStrategy,
                TotalStops = load.TotalStops,
                Weight = load.Weight,
                Volume = load.Volume,
                LoadType = load.LoadType,
                SpecialRequirements = load.SpecialRequirements,
                TotalDistanceKm = load.TotalDistanceKm,
                EstimatedTotalDurationMinutes = load.EstimatedTotalDurationMinutes,
                EarliestPickupTime = load.EarliestPickupTime,
                LatestDeliveryTime = load.LatestDeliveryTime,
                FixedPrice = load.FixedPrice,
                ContactPersonName = load.ContactPersonName,
                ContactPhone = load.ContactPhone,
                ContactEmail = load.ContactEmail,
                CreatedAt = load.CreatedDate,
                UpdatedAt = load.UpdatedDate,
                PublishedAt = load.PublishedAt,
                MatchedAt = load.MatchedAt,
                CompletedAt = load.CompletedAt,
                MatchedDriverId = load.MatchedDriverId,
                MatchedVehicleId = load.MatchedVehicleId,
                Dimensions = load.Dimensions != null ? new DimensionsDto
                {
                    Length = load.Dimensions.Length,
                    Width = load.Dimensions.Width,
                    Height = load.Dimensions.Height,
                    Unit = load.Dimensions.Unit
                } : null,
                LoadStops = load.LoadStops.OrderBy(s => s.StopOrder).Select(s => new LoadStopResponseDto
                {
                    Id = s.Id,
                    LoadId = s.LoadId,
                    StopOrder = s.StopOrder,
                    StopType = s.StopType,
                    Location = s.Location != null ? new LocationDto
                    {
                        Latitude = s.Location.Latitude,
                        Longitude = s.Location.Longitude,
                        Address = s.Location.Address,
                        City = s.Location.City,
                        District = s.Location.District,
                        PostalCode = s.Location.PostalCode,
                        Country = s.Location.Country,
                        LocationName = s.Location.LocationName,
                        AccessInstructions = s.Location.AccessInstructions,
                        ContactPerson = s.Location.ContactPerson,
                        ContactPhone = s.Location.ContactPhone
                    } : null!,
                    EarliestTime = s.EarliestTime,
                    LatestTime = s.LatestTime,
                    PlannedTime = s.PlannedTime,
                    EstimatedDurationMinutes = s.EstimatedDurationMinutes,
                    PickupWeight = s.PickupWeight,
                    DeliveryWeight = s.DeliveryWeight,
                    PickupVolume = s.PickupVolume,
                    DeliveryVolume = s.DeliveryVolume,
                    LoadDescription = s.LoadDescription,
                    SpecialInstructions = s.SpecialInstructions,
                    SpecialRequirements = s.SpecialRequirements,
                    ContactPersonName = s.ContactPersonName,
                    ContactPhone = s.ContactPhone,
                    ContactEmail = s.ContactEmail,
                    Status = s.Status,
                    ActualArrivalTime = s.ActualArrivalTime,
                    ActualDepartureTime = s.ActualDepartureTime,
                    CompletionNotes = s.CompletionNotes
                }).ToList()
            };
        }

        private LoadListItemDto MapToListItemDto(Load load)
        {
            var firstStop = load.LoadStops.OrderBy(s => s.StopOrder).FirstOrDefault();
            var lastStop = load.LoadStops.OrderByDescending(s => s.StopOrder).FirstOrDefault();

            return new LoadListItemDto
            {
                Id = load.Id,
                CompanyId = load.CompanyId,
                Title = load.Title,
                Status = load.Status,
                LoadCode = load.LoadCode,
                IsMultiStop = load.IsMultiStop,
                TotalStops = load.TotalStops,
                Weight = load.Weight,
                LoadType = load.LoadType,
                OriginCity = firstStop?.Location?.City,
                DestinationCity = lastStop?.Location?.City,
                TotalDistanceKm = load.TotalDistanceKm,
                FixedPrice = load.FixedPrice,
                EarliestPickupTime = load.EarliestPickupTime,
                LatestDeliveryTime = load.LatestDeliveryTime,
                CreatedAt = load.CreatedDate,
                PublishedAt = load.PublishedAt
            };
        }

        #endregion
    }

    /// <summary>
    /// DTO for cancelling a load
    /// </summary>
    public class CancelLoadDto
    {
        public string? Reason { get; set; }
    }
}
