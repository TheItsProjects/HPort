using ItsNameless.HPort.Repositories;
using ItsNameless.HPort.Test.Utils;

namespace ItsNameless.HPort.Test.Repositories;

[TestFixture]
[TestOf(typeof(ServerRepository))]
public class ServerRepositoryTest
{
    private Fakers _fakers;

    [SetUp]
    public void Setup()
    {
        _fakers = new Fakers();
    }

    [Test]
    public void GetServerName_WithoutUniqueServer_ReturnsCorrectName()
    {
        // Arrange
        var containerName = _fakers.Faker.Company.CompanyName();
        var serverType = _fakers.PortServerFaker.Generate().Type;
        var datacenter = _fakers.PortServerFaker.Generate().Datacenter;

        var expectedName = $"{serverType.ToString()}-{datacenter.ToString()}";


        // Act
        var actualName =
            ServerRepository.Name.GetServerName(
                containerName,
                serverType,
                datacenter,
                uniqueServer: false
            );

        //Assert
        Assert.That(actualName, Is.EqualTo(expectedName));
    }

    [Test]
    public void GetServerName_WithUniqueServer_ReturnsCorrectName()
    {
        // Arrange
        var containerName = _fakers.Faker.Company.CompanyName();
        var serverType = _fakers.PortServerFaker.Generate().Type;
        var datacenter = _fakers.PortServerFaker.Generate().Datacenter;

        var expectedName =
            $"{containerName}-{serverType.ToString()}-{datacenter.ToString()}";


        // Act
        var actualName =
            ServerRepository.Name.GetServerName(
                containerName,
                serverType,
                datacenter,
                uniqueServer: true
            );

        //Assert
        Assert.That(actualName, Is.EqualTo(expectedName));
    }
}
