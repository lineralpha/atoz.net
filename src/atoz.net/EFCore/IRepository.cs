using System.Linq.Expressions;

namespace Atoz.EFCore;

public interface IRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : class, IEntity
{
}

public interface IRepository<TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
{
    Task<bool> ExistsAsync(TPrimaryKey id);
    Task<TEntity?> GetByIdAsync(TPrimaryKey id);

    Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetByFilterAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);

    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);

    void Delete(TEntity entity, bool softDelete = true);
    Task DeleteAsync(TPrimaryKey id, bool softDelete = true);

    Task<bool> SaveChangesAsync();
}
