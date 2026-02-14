using NSubstitute;
using NSubstitute.Core;

namespace ItsNameless.HPort.Test.Utils.Extensions;

/// <summary>
/// Extensions used for NSubstitutes Substitute.
/// </summary>
public static class SubstituteExtensions
{
    /// <summary>
    /// Set a return value for this async call made with any arguments.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="returnThis">Value to return.</param>
    /// <param name="returnThese">Optionally, return these values next.</param>
    /// <typeparam name="T">The type to return as a Task.</typeparam>
    /// <returns>A <see cref="ConfiguredCall"/>.</returns>
    public static ConfiguredCall ReturnsAsyncForAnyArgs<T>(
        this Task<T> value,
        T returnThis,
        params T[] returnThese)
    {
        return value.ReturnsForAnyArgs(
            Task.FromResult(returnThis),
            returnThese.Select(Task.FromResult).ToArray()
        );
    }

    /// <summary>
    /// Set a return value for this async call made with specific arguments.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="returnThis">Value to return.</param>
    /// <param name="returnThese">Optionally, return these values next.</param>
    /// <typeparam name="T">The type to return as a Task.</typeparam>
    /// <returns>A <see cref="ConfiguredCall"/>.</returns>
    public static ConfiguredCall ReturnsAsync<T>(
        this Task<T> value,
        T returnThis,
        params T[] returnThese)
    {
        return value.Returns(
            Task.FromResult(returnThis),
            returnThese.Select(Task.FromResult).ToArray()
        );
    }
}
