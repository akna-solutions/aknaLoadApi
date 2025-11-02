namespace AknaLoad.Domain.Entities.ValueObjects
{
    public class Dimensions
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string Unit { get; set; } = "M"; // M for meters, CM for centimeters

        public Dimensions() { }

        public Dimensions(decimal length, decimal width, decimal height, string unit = "M")
        {
            Length = length;
            Width = width;
            Height = height;
            Unit = unit;
        }

        public decimal Volume => Length * Width * Height;

        public bool FitsIn(Dimensions containerDimensions)
        {
            if (Unit != containerDimensions.Unit)
            {
                // Convert to same unit for comparison
                var normalized = ConvertToMeters();
                var containerNormalized = containerDimensions.ConvertToMeters();
                return normalized.Length <= containerNormalized.Length &&
                       normalized.Width <= containerNormalized.Width &&
                       normalized.Height <= containerNormalized.Height;
            }

            return Length <= containerDimensions.Length &&
                   Width <= containerDimensions.Width &&
                   Height <= containerDimensions.Height;
        }

        private Dimensions ConvertToMeters()
        {
            if (Unit == "CM")
            {
                return new Dimensions(Length / 100, Width / 100, Height / 100, "M");
            }
            return this;
        }
    }
}