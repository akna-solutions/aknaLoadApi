using AknaLoad.Domain.Dtos.Requests;
using AknaLoad.Domain.Entities;
using AknaLoad.Domain.Enums;
using AknaLoad.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AknaLoad.API.Controllers
{
    [ApiController]
    [Route("api/matches")]
    public class MatchController : ControllerBase
    {
        private readonly IMatchingService _matchingService;

        public MatchController(IMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        /// <summary>
        /// Find matches for a specific load
        /// </summary>
        [HttpGet("load/{loadId}")]
        public async Task<ActionResult<List<Match>>> FindMatchesForLoad(long loadId, [FromQuery] int maxMatches = 10)
        {
            try
            {
                var matches = await _matchingService.FindMatchesForLoadAsync(loadId, maxMatches);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Find matches for a specific driver
        /// </summary>
        [HttpGet("driver/{driverId}")]
        public async Task<ActionResult<List<Match>>> FindMatchesForDriver(long driverId, [FromQuery] int maxMatches = 10)
        {
            try
            {
                var matches = await _matchingService.FindMatchesForDriverAsync(driverId, maxMatches);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get match by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Match>> GetMatchById(long id)
        {
            try
            {
                var match = await _matchingService.GetMatchByIdAsync(id);
                if (match == null)
                    return NotFound(new { message = "Match not found" });

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get match by code
        /// </summary>
        [HttpGet("code/{matchCode}")]
        public async Task<ActionResult<Match>> GetMatchByCode(string matchCode)
        {
            try
            {
                var match = await _matchingService.GetMatchByCodeAsync(matchCode);
                if (match == null)
                    return NotFound(new { message = "Match not found" });

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a manual match between load and driver
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Match>> CreateMatch([FromBody] CreateMatchRequest request)
        {
            try
            {
                var createdBy = User?.Identity?.Name ?? "Anonymous";
                var match = await _matchingService.CreateMatchAsync(
                    request.LoadId,
                    request.DriverId,
                    request.VehicleId,
                    createdBy);

                return CreatedAtAction(nameof(GetMatchById), new { id = match.Id }, match);
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
        /// Driver accepts a match
        /// </summary>
        [HttpPost("{id}/accept")]
        public async Task<ActionResult> AcceptMatch(long id)
        {
            try
            {
                var acceptedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _matchingService.AcceptMatchAsync(id, acceptedBy);

                if (!result)
                    return BadRequest(new { message = "Cannot accept match. Check match status." });

                return Ok(new { message = "Match accepted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Driver rejects a match
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<ActionResult> RejectMatch(long id, [FromBody] RejectMatchRequest request)
        {
            try
            {
                var rejectedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _matchingService.RejectMatchAsync(id, request.Reason, rejectedBy);

                if (!result)
                    return BadRequest(new { message = "Cannot reject match. Check match status." });

                return Ok(new { message = "Match rejected successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Load owner confirms a match
        /// </summary>
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult> ConfirmMatch(long id)
        {
            try
            {
                var confirmedBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _matchingService.ConfirmMatchAsync(id, confirmedBy);

                if (!result)
                    return BadRequest(new { message = "Cannot confirm match. Check match status." });

                return Ok(new { message = "Match confirmed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a match
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelMatch(long id, [FromBody] CancelMatchRequest request)
        {
            try
            {
                var cancelledBy = User?.Identity?.Name ?? "Anonymous";
                var result = await _matchingService.CancelMatchAsync(id, request.Reason, cancelledBy);

                if (!result)
                    return BadRequest(new { message = "Cannot cancel match. Check match status." });

                return Ok(new { message = "Match cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all matches for a load with optional status filter
        /// </summary>
        [HttpGet("load/{loadId}/all")]
        public async Task<ActionResult<List<Match>>> GetMatchesForLoad(long loadId, [FromQuery] MatchStatus? status = null)
        {
            try
            {
                var matches = await _matchingService.GetMatchesForLoadAsync(loadId, status);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all matches for a driver with optional status filter
        /// </summary>
        [HttpGet("driver/{driverId}/all")]
        public async Task<ActionResult<List<Match>>> GetMatchesForDriver(long driverId, [FromQuery] MatchStatus? status = null)
        {
            try
            {
                var matches = await _matchingService.GetMatchesForDriverAsync(driverId, status);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get pending matches for a driver
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<List<Match>>> GetPendingMatches([FromQuery] long? driverId = null)
        {
            try
            {
                var matches = await _matchingService.GetPendingMatchesAsync(driverId);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get expired matches
        /// </summary>
        [HttpGet("expired")]
        public async Task<ActionResult<List<Match>>> GetExpiredMatches()
        {
            try
            {
                var matches = await _matchingService.GetExpiredMatchesAsync();
                return Ok(matches);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get active match for a load
        /// </summary>
        [HttpGet("load/{loadId}/active")]
        public async Task<ActionResult<Match>> GetActiveMatchByLoad(long loadId)
        {
            try
            {
                var match = await _matchingService.GetActiveMatchByLoadAsync(loadId);
                if (match == null)
                    return NotFound(new { message = "No active match found for this load" });

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get active match for a driver
        /// </summary>
        [HttpGet("driver/{driverId}/active")]
        public async Task<ActionResult<Match>> GetActiveMatchByDriver(long driverId)
        {
            try
            {
                var match = await _matchingService.GetActiveMatchByDriverAsync(driverId);
                if (match == null)
                    return NotFound(new { message = "No active match found for this driver" });

                return Ok(match);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Process expired matches (usually called by background service)
        /// </summary>
        [HttpPost("process-expired")]
        public async Task<ActionResult> ProcessExpiredMatches()
        {
            try
            {
                await _matchingService.ProcessExpiredMatchesAsync();
                return Ok(new { message = "Expired matches processed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Notify driver about a match (send push notification, SMS, etc.)
        /// </summary>
        [HttpPost("{id}/notify")]
        public async Task<ActionResult> NotifyDriver(long id)
        {
            try
            {
                var result = await _matchingService.NotifyDriverAsync(id);

                if (!result)
                    return BadRequest(new { message = "Cannot notify driver. Check match status." });

                return Ok(new { message = "Driver notified successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Calculate match score between load and driver (for testing/debugging)
        /// </summary>
        [HttpPost("calculate-score")]
        public async Task<ActionResult<decimal>> CalculateMatchScore([FromBody] CalculateScoreRequest request)
        {
            try
            {
                var score = await _matchingService.CalculateMatchScoreAsync(request.Load, request.Driver, request.Vehicle);
                return Ok(new { matchScore = score });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
  
}