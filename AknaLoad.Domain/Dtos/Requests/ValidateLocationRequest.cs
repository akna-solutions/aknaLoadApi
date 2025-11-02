

using AknaLoad.Domain.Entities.ValueObjects;

namespace AknaLoad.Domain.Dtos.Requests
{

    public class ValidateLocationRequest
    {
        public Location CurrentLocation { get; set; } = null!;
        public decimal ToleranceMeters { get; set; } = 100;
    }
}
