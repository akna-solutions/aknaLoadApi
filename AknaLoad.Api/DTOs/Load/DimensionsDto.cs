namespace AknaLoad.Api.DTOs.Load
{
    /// <summary>
    /// Dimensions DTO
    /// </summary>
    public class DimensionsDto
    {
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string Unit { get; set; } = "M"; // M for meters, CM for centimeters
    }
}
