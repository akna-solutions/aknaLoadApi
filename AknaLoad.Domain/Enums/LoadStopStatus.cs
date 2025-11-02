

namespace AknaLoad.Domain.Enums
{
    /// <summary>
    /// Load stop status during execution
    /// </summary>
    public enum LoadStopStatus
    {
        Planned = 1,        // Planlandı
        InProgress = 2,     // Yolda (bu durağa doğru)
        Arrived = 3,        // Ulaşıldı
        Loading = 4,        // Yükleme/boşaltma yapılıyor
        Completed = 5,      // Tamamlandı
        Skipped = 6,        // Atlandı
        Failed = 7,         // Başarısız (müşteri yok, adres yanlış vs.)
        Delayed = 8         // Gecikme
    }
}
