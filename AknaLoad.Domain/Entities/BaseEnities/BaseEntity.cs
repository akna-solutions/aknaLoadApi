namespace AknaLoad.Domain.Entities.BaseEnities
{
    /// <summary>
    /// Tüm varlıklar için temel özellikleri içeren sınıf. 
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Varlığın benzersiz kimliği.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Varlığın oluşturulma tarihi.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Varlığı oluşturan kullanıcı.
        /// </summary>
        public string CreatedUser { get; set; }

        /// <summary>
        /// Varlığın güncellenme tarihi.
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// Varlığı güncelleyen kullanıcı.
        /// </summary>
        public string UpdatedUser { get; set; }

        /// <summary>
        /// Varlığın silinip silinmediğini belirtir.
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}