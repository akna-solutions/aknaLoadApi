using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using AknaLoad.Domain.Dtos.Requests;

namespace AknaLoad.API.Controllers
{
    [ApiController]
    [Route("api/trackings")]
    public class TrackingController : ControllerBase
    {
        private readonly ITrackingService _trackingService;

        public TrackingController(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

        /// <summary>
        /// Start tracking for a load
        /// </summary>
        [HttpPost("start")]
        public async Task<ActionResult<LoadTracking>> StartTracking([FromBody] StartTrackingRequest request)
        {
            try
            {
                var startedBy = User?.Identity?.Name ?? "Anonymous";
                var tracking = await _trackingService.StartTrackingAsync(
                    request.LoadId,
                    request.DriverId,
                    request.MatchId,
                    startedBy);

                return Ok(tracking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update location for a load
        /// </summary>
        [HttpPost("{loadId}/location")]
        public async Task<ActionResult> UpdateLocation(long loadId, [FromBody] UpdateLocationRequest request)
        {
            try
            {
                var updatedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _trackingService.UpdateLocationAsync(
                    loadId,
                    request.Location,
                    request.Speed,
                    request.Heading,
                    updatedBy);

                if (!result)
                    return BadRequest(new { message = "Could not update location" });

                return Ok(new { message = "Location updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update status for a load
        /// </summary>
        [HttpPost("{loadId}/status")]
        public async Task<ActionResult> UpdateStatus(long loadId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var updatedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _trackingService.UpdateStatusAsync(
                    loadId,
                    request.Status,
                    request.Notes,
                    updatedBy);

                if (!result)
                    return BadRequest(new { message = "Could not update status" });

                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get latest tracking information for a load
        /// </summary>
        [HttpGet("{loadId}/latest")]
        public async Task<ActionResult<LoadTracking>> GetLatestTracking(long loadId)
        {
            try
            {
                var tracking = await _trackingService.GetLatestTrackingAsync(loadId);
                if (tracking == null)
                    return NotFound(new { message = "No tracking information found" });

                return Ok(tracking);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get tracking history for a load
        /// </summary>
        [HttpGet("{loadId}/history")]
        public async Task<ActionResult<List<LoadTracking>>> GetTrackingHistory(long loadId)
        {
            try
            {
                var history = await _trackingService.GetTrackingHistoryAsync(loadId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all active trackings
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<LoadTracking>>> GetActiveTrackings()
        {
            try
            {
                var trackings = await _trackingService.GetActiveTrackingsAsync();
                return Ok(trackings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Report an exception/problem during transport
        /// </summary>
        [HttpPost("{loadId}/exception")]
        public async Task<ActionResult> ReportException(long loadId, [FromBody] ReportExceptionRequest request)
        {
            try
            {
                var reportedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _trackingService.ReportExceptionAsync(
                    loadId,
                    request.ExceptionType,
                    request.Description,
                    reportedBy);

                if (!result)
                    return BadRequest(new { message = "Could not report exception" });

                return Ok(new { message = "Exception reported successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Resolve an exception
        /// </summary>
        [HttpPost("exception/{trackingId}/resolve")]
        public async Task<ActionResult> ResolveException(long trackingId)
        {
            try
            {
                var resolvedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _trackingService.ResolveExceptionAsync(trackingId, resolvedBy);

                if (!result)
                    return BadRequest(new { message = "Could not resolve exception" });

                return Ok(new { message = "Exception resolved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Complete delivery with digital signature
        /// </summary>
        [HttpPost("{loadId}/complete")]
        public async Task<ActionResult> CompleteDelivery(long loadId, [FromBody] CompleteDeliveryRequest request)
        {
            try
            {
                var completedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _trackingService.CompleteDeliveryAsync(
                    loadId,
                    request.Signature,
                    request.RecipientName,
                    request.RecipientIdNumber,
                    completedBy);

                if (!result)
                    return BadRequest(new { message = "Could not complete delivery" });

                return Ok(new { message = "Delivery completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add photos to a load tracking
        /// </summary>
        [HttpPost("{loadId}/photos")]
        public async Task<ActionResult> AddPhotos(long loadId, [FromBody] AddPhotosRequest request)
        {
            try
            {
                var addedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _trackingService.AddPhotosAsync(loadId, request.PhotoUrls, addedBy);

                if (!result)
                    return BadRequest(new { message = "Could not add photos" });

                return Ok(new { message = "Photos added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add documents to a load tracking
        /// </summary>
        [HttpPost("{loadId}/documents")]
        public async Task<ActionResult> AddDocuments(long loadId, [FromBody] AddDocumentsRequest request)
        {
            try
            {
                var addedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _trackingService.AddDocumentsAsync(loadId, request.DocumentUrls, addedBy);

                if (!result)
                    return BadRequest(new { message = "Could not add documents" });

                return Ok(new { message = "Documents added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get delayed loads
        /// </summary>
        [HttpGet("delayed")]
        public async Task<ActionResult<List<LoadTracking>>> GetDelayedLoads([FromQuery] int delayThresholdMinutes = 30)
        {
            try
            {
                var delayedLoads = await _trackingService.GetDelayedLoadsAsync(delayThresholdMinutes);
                return Ok(delayedLoads);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get loads that are off route
        /// </summary>
        [HttpGet("off-route")]
        public async Task<ActionResult<List<LoadTracking>>> GetOffRouteLoads([FromQuery] decimal deviationThresholdKm = 5.0m)
        {
            try
            {
                var offRouteLoads = await _trackingService.GetOffRouteLoadsAsync(deviationThresholdKm);
                return Ok(offRouteLoads);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get trackings with exceptions
        /// </summary>
        [HttpGet("exceptions")]
        public async Task<ActionResult<List<LoadTracking>>> GetTrackingsWithExceptions([FromQuery] bool unresolvedOnly = true)
        {
            try
            {
                var trackings = await _trackingService.GetTrackingsWithExceptionsAsync(unresolvedOnly);
                return Ok(trackings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Calculate progress percentage for a load
        /// </summary>
        [HttpGet("{loadId}/progress")]
        public async Task<ActionResult<decimal>> CalculateProgress(long loadId)
        {
            try
            {
                var progress = await _trackingService.CalculateProgressPercentageAsync(loadId);
                return Ok(new { progressPercentage = progress });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get estimated arrival time for a load
        /// </summary>
        [HttpGet("{loadId}/eta")]
        public async Task<ActionResult<DateTime?>> GetEstimatedArrivalTime(long loadId)
        {
            try
            {
                var eta = await _trackingService.GetEstimatedArrivalTimeAsync(loadId);
                return Ok(new { estimatedArrivalTime = eta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Send location notification to stakeholders
        /// </summary>
        [HttpPost("{loadId}/notify")]
        public async Task<ActionResult> SendLocationNotification(long loadId, [FromBody] NotificationRequest request)
        {
            try
            {
                var result = await _trackingService.SendLocationNotificationAsync(loadId, request.RecipientType);

                if (!result)
                    return BadRequest(new { message = "Could not send notification" });

                return Ok(new { message = "Notification sent successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get delivery performance metrics
        /// </summary>
        [HttpGet("performance")]
        public async Task<ActionResult<decimal>> GetDeliveryPerformance(
            [FromQuery] long? driverId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var performance = await _trackingService.GetDeliveryPerformanceAsync(driverId, fromDate, toDate);
                return Ok(new { deliveryPerformance = performance });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Validate delivery location
        /// </summary>
        [HttpPost("{loadId}/validate-location")]
        public async Task<ActionResult<bool>> ValidateDeliveryLocation(long loadId, [FromBody] ValidateLocationRequest request)
        {
            try
            {
                var isValid = await _trackingService.ValidateDeliveryLocationAsync(
                    loadId,
                    request.CurrentLocation,
                    request.ToleranceMeters);

                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Search trackings with filters
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<LoadTracking>>> SearchTrackings(
            [FromQuery] TrackingStatus? status = null,
            [FromQuery] long? loadId = null,
            [FromQuery] long? driverId = null,
            [FromQuery] bool? hasException = null,
            [FromQuery] bool? isOnTime = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var trackings = await _trackingService.SearchTrackingsAsync(
                    status, loadId, driverId, hasException, isOnTime,
                    fromDate, toDate, pageNumber, pageSize);

                return Ok(trackings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}