namespace AknaLoad.Domain.Dtos.Requests
{
    public class AddPhotosRequest
    {
        public List<string> PhotoUrls { get; set; } = new();
    }
}
