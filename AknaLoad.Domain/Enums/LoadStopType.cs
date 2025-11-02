

namespace AknaLoad.Domain.Enums
{
    /// <summary>
    /// Load stop types - pickup, delivery or both
    /// </summary>
    public enum LoadStopType
    {
        Pickup = 1,     // Sadece yük alma noktası
        Delivery = 2,   // Sadece yük bırakma noktası
        Both = 3        // Hem alma hem bırakma (transfer noktası)
    }
}
