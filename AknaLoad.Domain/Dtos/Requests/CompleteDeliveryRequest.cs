
namespace AknaLoad.Domain.Dtos.Requests
{
    public class CompleteDeliveryRequest
    {
        public string Signature { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string? RecipientIdNumber { get; set; }
    }
}
