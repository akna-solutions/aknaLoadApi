namespace AknaLoad.Domain.Entities.ValueObjects
{
    public class Location
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

        public Location() { }

        public Location(decimal latitude, decimal longitude, string address, string city)
        {
            Latitude = latitude;
            Longitude = longitude;
            Address = address;
            City = city;
        }

        public double DistanceTo(Location other)
        {
            const double R = 6371; // Earth's radius in km
            double dLat = ToRadians((double)(other.Latitude - Latitude));
            double dLon = ToRadians((double)(other.Longitude - Longitude));

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(ToRadians((double)Latitude)) * Math.Cos(ToRadians((double)other.Latitude)) *
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}