using AknaLoad.Domain.Entities.ValueObjects;

namespace AknaLoad.Domain.Dtos
{
    public class DimensionsDto
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string Unit { get; set; } = "M";

        public Dimensions ToDimensions()
        {
            return new Dimensions(Length, Width, Height, Unit);
        }
    }
}
