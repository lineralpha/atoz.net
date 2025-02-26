using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atoz.EFCore;
public abstract class ExtendedDbContext : DbContext
{
    /// <inheritdoc/>
    public ExtendedDbContext()
        : base()
    {
    }

    /// <inheritdoc/>
    public ExtendedDbContext(DbContextOptions options)
        : base(options)
    {
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureFilters(modelBuilder);
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    /// <inheritdoc/>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected virtual void ConfigureFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            LazyConfigureFiltersCoreMethodInfo.Value
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [modelBuilder, entityType]);
        }
    }

    private static Lazy<MethodInfo> LazyConfigureFiltersCoreMethodInfo
        => new(() => GetPrivateStaticConfigureFiltersCoreMethodInfo()!);

    // 
    private static MethodInfo? GetPrivateStaticConfigureFiltersCoreMethodInfo()
    {
        return
            typeof(ExtendedDbContext).GetMethod(
                nameof(ConfigureFiltersCore),
                1,  // count of generic type parameters
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                [typeof(ModelBuilder), typeof(IMutableEntityType)],
                null);
    }

    /// <summary>
    /// Configure global query filters on the table mapping to <typeparamref name="TEntity"/>.
    /// </summary>
    private static void ConfigureFiltersCore<TEntity>(ModelBuilder modelBuilder, IMutableEntityType entityType)
        where TEntity : class
    {
        Debug.Assert(typeof(TEntity) == entityType.ClrType);

        if (typeof(TEntity).IsAssignableTo(typeof(ISoftDelete)))
        {
            Expression<Func<TEntity, bool>> filterExpression =
                entity => !((ISoftDelete)entity).IsDeleted;

            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(filterExpression)
                .Property(e => ((ISoftDelete)e).IsDeleted)
                .HasDefaultValue(false);
        }

        if (typeof(TEntity).IsAssignableTo(typeof(IAuditable)))
        {
            modelBuilder.Entity<TEntity>()
                .Ignore(e => ((IAuditable)e).CreatedAt)
                .Ignore(e => ((IAuditable)e).CreatedBy)
                .Ignore(e => ((IAuditable)e).LastModifiedAt)
                .Ignore(e => ((IAuditable)e).LastModifiedBy)
                .Ignore(e => ((IAuditable)e).DeletedAt)
                .Ignore(e => ((IAuditable)e).DeletedBy);
        }
    }
}
