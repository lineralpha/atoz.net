namespace Atoz;

/// <summary>
/// Provides a thread-safe framework for derived classes to implement dispose pattern.
/// <para />
/// <see cref="Dispose"/> and <see cref="DisposeAsync()"/> are thread-safe. It is guaranteed that
/// only one thread can enter <see cref="DisposeAsync(bool)"/>.
/// </summary>
public abstract class ThreadSafeDisposable : IDisposable, IAsyncDisposable
{
    // 1 = disposed, 0 = not disposed
    private long _isDisposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            DisposeAsync(true).RunSynchronously2();
            GC.SuppressFinalize(this);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (0 == Interlocked.CompareExchange(ref _isDisposed, 1, 0))
        {
            await DisposeAsync(true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Implements resource cleanup logic. This method is called only once during object disposal.
    /// </summary>
    /// <param name="disposing">
    ///     Indicates whether this method was invoked from the <c>Dispose</c> methods (<c>true</c>)
    ///     or from the GC Finalizer (<c>false</c>).
    /// </param>
    /// <remarks>
    ///     If this method was invoked from the GC Finalizer (<paramref name="disposing"/> is <c>false</c>),
    ///     only unmananged resources hold by this object should be cleaned up.
    ///   <para />
    ///     If it was invoked from the <c>Dispose</c> methods (<paramref name="disposing"/> is <c>true</c>),
    ///     all unmanaged and mananged resources hold by this object should be disposed as well.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the dispose operation.</returns>
    protected abstract Task DisposeAsync(bool disposing);

    /// <summary>
    /// Throws <see cref="ObjectDisposedException" /> if this object is disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(1 == Interlocked.Read(ref _isDisposed), this);
    }

    /// <summary>
    /// Indicates whether this object is disposed.
    /// </summary>
    protected bool IsDisposed => 1 == Interlocked.Read(ref _isDisposed);
}
