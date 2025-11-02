using AknaLoad.Domain.Entities.ValueObjects;

namespace AknaLoad.Domain.Dtos.Requests
{
    public class UpdateLocationRequest
    {
        public Location Location { get; set; } = null!;
        public decimal? Speed { get; set; }
        public int? Heading { get; set; }
    }
}
