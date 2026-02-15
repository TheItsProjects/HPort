using ItsNameless.HPort.Extensions;

namespace ItsNameless.HPort.Test.Extensions;

[TestFixture]
[TestOf(typeof(AsyncEnumerableExtensions))]
public class AsyncEnumerableExtensionsTest
{
    [Test]
    public async Task ToListAsync_ReceivesAsyncEnumerable_ReturnsList()
    {
        // Arrange
        var elements = new List<int> { 1, 2, 3, };
        var asyncEnumerable = CreateAsyncEnumerable(elements);

        // Act
        var result = await asyncEnumerable.ToListAsync();

        // Assert
        Assert.That(result, Is.EqualTo(elements));
    }

    [Test]
    public async Task SingleAsync_ReceivesOneElement_ReturnsElement()
    {
        // Arrange
        var elements = new List<int> { 1, };
        var asyncEnumerable = CreateAsyncEnumerable(elements);

        // Act
        var result = await asyncEnumerable.SingleAsync();

        // Assert
        Assert.That(result, Is.EqualTo(elements[0]));
    }

    [Test]
    public void SingleAsync_ReceivesTwoElements_Throws()
    {
        // Arrange
        var elements = new List<int> { 1, 2, };
        var asyncEnumerable = CreateAsyncEnumerable(elements);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => asyncEnumerable.SingleAsync());
    }

    private static async IAsyncEnumerable<int> CreateAsyncEnumerable(
        List<int> elements)
    {
        foreach (var element in elements)
        {
            yield return element;
        }
    }
}
