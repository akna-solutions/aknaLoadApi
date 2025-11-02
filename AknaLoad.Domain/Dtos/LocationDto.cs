using AknaLoad.Domain.Entities.ValueObjects;

namespace AknaLoad.Domain.Dtos
{
    /// <summary>
    /// DTO for location information
    /// </summary>
    public class LocationDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "TR";
        public string? LocationName { get; set; }
        public string? AccessInstructions { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }

        public Location ToLocation()
        {
            return new Location(Latitude, Longitude, Address, City)
            {
                District = District,
                PostalCode = PostalCode,
                Country = Country,
                LocationName = LocationName,
                AccessInstructions = AccessInstructions,
                ContactPerson = ContactPerson,
                ContactPhone = ContactPhone
            };
        }


    }
}
