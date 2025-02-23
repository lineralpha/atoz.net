namespace Atoz;

public static class TaskExtensions
{
    /// <summary>
    /// Runs the <see cref="Task"/> in "fire and forget" mode.
    /// <para />
    /// The <paramref name="task"/> runs in a separate thread, and the current thread is not blocking.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> to run.</param>
    public static void FireAndForget(this Task task)
    {
        Task.Run(async () =>
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    /// <summary>
    /// Runs the <see cref="Task{T}"/> in "fire and forget" mode.
    /// <para />
    /// The <paramref name="task"/> runs in a separate thread, and the current thread is not blocking.
    /// </summary>
    /// <typeparam name="T">The type of the result from the <see cref="Task{T}"/></typeparam>
    /// <param name="task">The <see cref="Task{T}"/> to run.</param>
    public static void FireAndForget<T>(this Task<T> task)
    {
        Task.Run(async () =>
        {
            try
            {
                // Result from the task is discarded.
                T r = await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    /// <summary>
    /// Runs the <see cref="Task"/> synchronously by blocking the current thread till the task completion.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> to run.</param>
    public static void RunSynchronously2(this Task task)
    {
        try
        {
            task.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Runs the <see cref="Task{T}"/> synchronously by blocking the current thread till the task completion.
    /// </summary>
    /// <typeparam name="T">The type of the result returned from the <see cref="Task{T}"/> upon completion.</typeparam>
    /// <param name="task">The <see cref="Task{T}"/> to run.</param>
    /// <returns>The result from the task if it runs to completion; <c>default(T)</c> if task was cancelled. </returns>
    public static T? RunSynchronously2<T>(this Task<T> task)
    {
        try
        {
            return task.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            return default;
        }
    }
}
