namespace AknaLoad.Domain.Enums
{
    /// <summary>
    /// Load routing strategy for multi-stop loads
    /// </summary>
    public enum LoadRoutingStrategy
    {
        Manual = 1,         // Manuel sıralama (kullanıcı belirlemiş)
        Optimized = 2,      // Otomatik optimize edilmiş rota
        Sequential = 3,     // Sıralı (önce tüm pickup'lar sonra delivery'ler)
        Flexible = 4        // Esnek (şoför karar verir)
    }
}
