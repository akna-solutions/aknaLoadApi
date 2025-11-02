
namespace AknaLoad.Domain.Dtos.Requests
{
    public class NotificationRequest
    {
        public string RecipientType { get; set; } = "customer"; // customer, driver, dispatcher
    }
}
