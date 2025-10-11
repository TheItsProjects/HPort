namespace ItsNameless.HPort.IntegrationTest;

[TestFixture]
[TestOf(typeof(HPort))]
public class SingleContainerTests : TestBase
{
    [Test, Order(1),]
    public async Task AddFirstContainer_NoServerExists_CreatesServer()
    {
        // Arrange
        await Log("Creating working files...");
        await File.WriteAllTextAsync(
            _testConfig.ComposeFileName,
            _testConfig.ComposeFileContent
        );
        await File.WriteAllTextAsync(
            _testConfig.EnvFileName,
            _testConfig.EnvFileContent
        );
        await Log("-> Working files created.");

        // Act
        var actualContainer =
            await _sut.Container.CreateContainer(
                _testConfig.Container.Name,
                _testConfig.Container.Server.Type,
                _testConfig.Container.Server.Datacenter,
                _testConfig.ComposeFileName,
                _testConfig.EnvFileName,
                uniqueServer: false
            );

        //Assert
        Assert.That(actualContainer, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                actualContainer.Name,
                Is.EqualTo(_testConfig.Container.Name)
            );
            Assert.That(actualContainer.Server, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                actualContainer.Server.Type,
                Is.EqualTo(_testConfig.Container.Server.Type)
            );
            Assert.That(
                actualContainer.Server.Datacenter,
                Is.EqualTo(_testConfig.Container.Server.Datacenter)
            );
        }
    }
}
