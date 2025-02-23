using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Atoz;

[DebuggerStepThrough]
public static class GuardedArgument
{
    /// <inheritdoc />
    public static T ThrowIfNull<T>(
        [NotNull] T? argument,
        [CallerArgumentExpression(nameof(argument))] string? argName = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(argument, argName);
        return argument;
    }

    /// <inheritdoc />
    public static string ThrowIfNullOrEmpty(
        [NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))] string? argName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(argument, argName);
        return argument;
    }

    /// <inheritdoc />
    public static string ThrowIfNullOrWhiteSpace(
        [NotNull] string? argument,
        [CallerArgumentExpression(nameof(argument))] string? argName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(argument, argName);
        return argument;
    }

    /// <summary>
    /// Ensures <paramref name="predicate"/> of <paramref name="argument"/> is evaluated to <see langword="true"/>.
    /// Otherwise, throws an exception of type <typeparamref name="TException"/>.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="argument"/>.</typeparam>
    /// <typeparam name="TException">
    ///     The type of exception to throw if <paramref name="predicate"/> is evaluated to <see langword="false"/>
    /// </typeparam>
    /// <param name="predicate">
    ///     The condition lambda which the <paramref name="argument"/> must meet to not throw.
    /// </param>
    /// <param name="argument">The argument to be evaluated.</param>
    /// <param name="message">The error message for the exception.</param>
    /// <param name="argName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <returns>The <paramref name="argument"/> without changing.</returns>
    public static T RequireOrThrow<T, TException>(
        Func<T, bool> predicate,
        T argument,
        string? message,
        [CallerArgumentExpression(nameof(argument))] string? argName = null)
        where TException : ArgumentException
    {
        ThrowIfNull(predicate);

        return predicate(argument)
            ? argument
            : throw (TException)Activator.CreateInstance(
                typeof(TException),
                message ?? $"Value '{argument}' does not meet requirement",
                argName)!;
    }
}
