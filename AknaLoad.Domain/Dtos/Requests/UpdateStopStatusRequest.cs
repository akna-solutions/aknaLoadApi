using AknaLoad.Domain.Enums;

namespace AknaLoad.Domain.Dtos.Requests
{
    /// <summary>
    /// Request for updating stop status during execution
    /// </summary>
    public class UpdateStopStatusRequest
    {
        public string Status { get; set; } = string.Empty; // Planned, InProgress, Arrived, Loading, Completed, Skipped, Failed, Delayed
        public string? Notes { get; set; }
        public DateTime? ActualTime { get; set; }

        public LoadStopStatus GetStatus()
        {
            return Enum.TryParse<LoadStopStatus>(Status, true, out var result) ? result : LoadStopStatus.Planned;
        }
    }
}
