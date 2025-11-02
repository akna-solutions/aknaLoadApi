

using AknaLoad.Domain.Entities;

namespace AknaLoad.Domain.Dtos.Requests
{
    public class CalculateScoreRequest
    {
        public Load Load { get; set; } = null!;
        public Driver Driver { get; set; } = null!;
        public VehicleDto? Vehicle { get; set; }
    }
}
