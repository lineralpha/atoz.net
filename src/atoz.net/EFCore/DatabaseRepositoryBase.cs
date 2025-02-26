using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Atoz.EFCore;

/// <summary>
/// General <see cref="IRepository{TEntity}"/> implementation for <see cref="IEntity"/> that uses an
/// <see langword="int"/> as its primary key.
/// </summary>
/// <typeparam name="TEntity">The <see cref="Type"/> of the entity.</typeparam>
public class DatabaseRepositoryBase<TEntity> : DatabaseRepositoryBase<TEntity, int>
    where TEntity : class, IEntity
{
    public DatabaseRepositoryBase(DbContext dbContext)
        : base(dbContext)
    {
    }
}

/// <summary>
/// General <see cref="IRepository{TEntity, TPrimaryKey}"/> implementation for <see cref="IEntity{T}"/>
/// that uses <typeparamref name="TPrimaryKey"/> as its primary key.
/// </summary>
/// <typeparam name="TEntity">The <see cref="Type"/> of the entity.</typeparam>
/// <typeparam name="TPrimaryKey">The <see cref="Type"/> of the entity's primary key.</typeparam>
public class DatabaseRepositoryBase<TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
{
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _entities;

    public DatabaseRepositoryBase(DbContext dbContext)
    {
        _dbContext = GuardedArgument.ThrowIfNull(dbContext);
        _entities = dbContext.Set<TEntity>();
    }

    protected IQueryable<TEntity> AsQueryable()
        => _entities.AsQueryable();

    public virtual async Task<bool> ExistsAsync(TPrimaryKey id)
    {
        return await _entities.AnyAsync(e => e.Id!.Equals(id)).ConfigureAwait(false);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TPrimaryKey id)
    {
        // FindAsync outperforms FirstOrDefaultAsync.
        // FindAsync is optimized to search by id.
        return await _entities.FindAsync(id).ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        // Warning: this returns massive data
        return await _entities
            .AsQueryable()
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<TEntity>> GetByFilterAsync(
        Expression<Func<TEntity, bool>> filter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        GuardedArgument.ThrowIfNull(filter);

        var query = _entities.AsQueryable();
        query = query.Where(filter);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.ToListAsync().ConfigureAwait(false);
    }

    public virtual Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null)
    {
        GuardedArgument.ThrowIfNull(predicate);

        var query = _entities.AsQueryable();
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        return query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<TEntity?> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        GuardedArgument.ThrowIfNull(predicate);

        return await _entities.SingleOrDefaultAsync(predicate).ConfigureAwait(false);
    }

    public virtual void Add(TEntity entity)
    {
        _entities.Add(entity);
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        _entities.AddRange(entities);
    }

    public virtual void Update(TEntity entity)
    {
        _entities.Update(entity);

        //_entities.Attach(entity);
        //_dbContext.Entry(entity).State = EntityState.Modified;
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        _entities.UpdateRange(entities);

        //_entities.AttachRange(entities);
        //foreach (var entity in entities)
        //{
        //    _dbContext.Entry(entity).State = EntityState.Modified;
        //}
    }

    public virtual void Delete(TEntity entity, bool softDelete = true)
    {
        if (softDelete)
        {
            if (entity is ISoftDelete softDeletable)
            {
                var entry = _dbContext.ChangeTracker
                    .Entries<ISoftDelete>()
                    .FirstOrDefault(entry => entry.Entity == entity);

                if (entry != null)
                {
                    softDeletable.IsDeleted = true;
                    Update(entity);
                }
                return;
            }

            throw new NotSupportedException(
                $"The entity type '{entity.GetType().Name}' does not support soft-delete." +
                $"It requires to implement {nameof(ISoftDelete)} to make it soft deletable.");
        }
        else
        {
            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                _entities.Attach(entity);
            }
            _entities.Remove(entity);
        }
    }

    public virtual async Task DeleteAsync(TPrimaryKey id, bool softDelete = true)
    {
        TEntity? entity = await _entities.FindAsync(id).ConfigureAwait(false);

        if (entity != null)
        {
            Delete(entity, softDelete);
        }
    }

    public virtual async Task<bool> SaveChangesAsync()
    {
        int count = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        return count > 0;
    }
}
