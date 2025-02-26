using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Atoz.EFCore;

// https://blog.bartdemeyer.be/2019/07/unit-testing-entity-framework-core-mocking-dbset-async-methods/

public static class AsyncQueryableExtensions
{
    /// <summary>
    /// DO NOT USE IN PRODUCT CODE.
    /// <see href="https://learn.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking"/> 
    ///
    /// The intention of this class is to create a mock for testing EFCore queries in unit tests.
    /// </summary>
    public static IQueryable<TEntity> AsAsyncQueryable<TEntity>(this IEnumerable<TEntity> source)
        => new MockAsyncQueryable<TEntity>(GuardedArgument.ThrowIfNull(source));
}

/// <summary>
/// DO NOT USE IN PRODUCT CODE.
/// <see href="https://learn.microsoft.com/en-us/ef/ef6/fundamentals/testing/mocking"/> 
///
/// The intention of this class is to create a mock for testing EFCore queries in unit tests.
/// </summary>
internal class MockAsyncQueryable<TEntity> : EnumerableQuery<TEntity>, IAsyncEnumerable<TEntity>, IQueryable<TEntity>
{
    public MockAsyncQueryable(IEnumerable<TEntity> enumerable)
        : base(enumerable)
    {
    }

    public MockAsyncQueryable(Expression expression)
        : base(expression)
    {
    }

    public IAsyncEnumerator<TEntity> GetEnumerator()
        => new AsyncEnumerator(this.AsEnumerable().GetEnumerator());

    public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => GetEnumerator();

    IQueryProvider IQueryable.Provider => new AsyncQueryProvider(this);

    private class AsyncEnumerator : IAsyncEnumerator<TEntity>
    {
        private readonly IEnumerator<TEntity> _inner;

        public AsyncEnumerator(IEnumerator<TEntity> inner)
        {
            _inner = inner;
        }

        public TEntity Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

#pragma warning disable CS1998 // Nothing to await
        public async ValueTask DisposeAsync() => _inner.Dispose();
#pragma warning restore CS1998
    }

    private class AsyncQueryProvider : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public AsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
            => new MockAsyncQueryable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new MockAsyncQueryable<TElement>(expression);

        public object? Execute(Expression expression)
            => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => new MockAsyncQueryable<TResult>(expression);

#pragma warning disable CS1066 // No cancellation needed
        TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken token = default)
            => Execute<TResult>(expression);
#pragma warning restore CS1066 
    }
}
