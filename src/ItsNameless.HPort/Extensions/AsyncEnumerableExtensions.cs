namespace ItsNameless.HPort.Extensions;

/// <summary>
/// Extensions for <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
internal static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Returns a list containing all elements of the source sequence.
    /// </summary>
    /// <param name="source">The sequence to convert to a list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <returns>A list containing all elements.</returns>
    internal static async Task<List<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// Returns the single element of the sequence, or throws an exception if the sequence is empty or contains more than one element.
    /// </summary>
    /// <param name="source">The sequence to extract the only element from.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <typeparam name="T">The type of the element.</typeparam>
    /// <returns>The single element.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the sequence is empty or contains more than one element.</exception>
    internal static async Task<T> SingleAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        await using var enumerator =
            source.GetAsyncEnumerator(cancellationToken);
        if (await enumerator.MoveNextAsync())
        {
            var result = enumerator.Current;
            if (await enumerator.MoveNextAsync())
            {
                throw new InvalidOperationException(
                    "Sequence contains more than one element"
                );
            }

            return result;
        }

        throw new InvalidOperationException("Sequence contains no elements");
    }
}
