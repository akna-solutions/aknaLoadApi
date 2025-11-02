namespace AknaLoad.Domain.Enums
{
    public enum TrackingStatus
    {
        WaitingForPickup = 1,
        PickedUp = 2,
        InTransit = 3,
        AtDeliveryLocation = 4,
        Delivered = 5,
        Exception = 6,
        Delayed = 7,
        OnHold = 8
    }
}