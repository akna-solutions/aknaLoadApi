using AknaLoad.Domain.Entities.BaseEnities;
using System.Linq.Expressions;

namespace AknaLoad.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interface for generic repository operations
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        // Create Operations
        Task<TEntity> AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // Read Operations
        Task<TEntity?> GetByIdAsync(long id, bool trackChanges = true);
        Task<TEntity?> GetFirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool trackChanges = true,
            params Expression<Func<TEntity, object>>[] includes);

        IQueryable<TEntity> GetAll(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool trackChanges = true);

        Task<List<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool trackChanges = true);

        // Count and Existence
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        // Update Operations
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);

        // Delete Operations
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        Task<bool> SoftDeleteAsync(long id);

        // Complex Query Methods
        IQueryable<TEntity> GetQueryable();
        Task<List<TEntity>> GetPagedListAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            int pageNumber = 1,
            int pageSize = 10,
            params Expression<Func<TEntity, object>>[] includes);
    }
}