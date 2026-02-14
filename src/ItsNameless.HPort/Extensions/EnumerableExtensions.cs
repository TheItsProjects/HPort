namespace ItsNameless.HPort.Extensions;

/// <summary>
/// Extensions for <see cref="IEnumerable{T}"/>.
/// </summary>
internal static class EnumerableExtensions
{
    /// <summary>
    /// Returns only the elements of the source sequence that are not null or empty strings.
    /// </summary>
    /// <returns>The elements of the sequence which are not null or empty.</returns>
    internal static IEnumerable<string> WhereNotEmptyOrNull(
        this IEnumerable<string?> source) =>
        source.Where(item => item is not null && item != string.Empty)!;
}
