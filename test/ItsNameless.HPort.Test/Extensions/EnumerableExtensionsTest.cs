using ItsNameless.HPort.Extensions;

namespace ItsNameless.HPort.Test.Extensions;

[TestFixture]
[TestOf(typeof(EnumerableExtensions))]
public class EnumerableExtensionsTest
{
    [Test]
    public void ReceivesDifferentElements_ReturnsCorrectElements()
    {
        // Arrange
        List<string?> elements = ["Hello", "World", null, "",];

        // Act
        var result = elements.WhereNotEmptyOrNull().ToList();

        //Assert
        Assert.That(result, Is.EqualTo(new List<string?> { "Hello", "World" }));
    }
}
