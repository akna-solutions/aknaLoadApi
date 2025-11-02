namespace AknaLoad.Domain.Interfaces.UnitOfWorks
{
    /// <summary>
    /// Interface for Unit of Work pattern
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}