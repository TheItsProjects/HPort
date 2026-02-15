namespace ItsNameless.HPort.IntegrationTest;

[TestFixture, Explicit]
[TestOf(typeof(HPort))]
public class SingleContainerTest : TestBase
{
    [Test, Order(1)]
    public async Task
        AddContainer_NoServerExists_CreatesServer_AndAddsContainer()
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
        await Log("Creating container...");
        var actualContainer =
            await _sut.Container.CreateContainer(
                _testConfig.Container.Name,
                _testConfig.Container.Server.Type,
                _testConfig.Container.Server.Datacenter,
                _testConfig.ComposeFileName,
                _testConfig.EnvFileName,
                uniqueServer: false
            );
        await Log("-> Container created.");

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

        await Log("Test completed.");
    }

    [Test, Order(2)]
    public async Task CheckContainer_RunsCommandInContainer_AndReturnsResult()
    {
        // Act
        // Wait some time to ensure the container is fully started.
        await Task.Delay(5000);

        await Log("Executing command in container...");
        var result =
            await _sut.Container.ExecuteCommandInContainer(
                _testConfig.Container.Name,
                _testConfig.Container.Server.Name,
                _testConfig.ServiceName,
                "nginx -v"
            );
        await Log("-> Command executed with result: " + result);

        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("nginx version: nginx/"));
        await Log("Test completed.");
    }

    [Test, Order(98)]
    public async Task DeleteContainer_DeletesContainer_AndServer()
    {
        // Act
        await Log("Deleting container...");
        var actualContainer =
            await _sut.Container.DeleteContainer(
                _testConfig.Container.Name,
                _testConfig.Container.Server.Name,
                true
            );
        await Log("-> Container deleted.");

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

        Assert.That(
            actualContainer.Server.Name,
            Is.EqualTo(_testConfig.Container.Server.Name)
        );
        await Log("Test completed.");
    }

    [Test, Order(99)]
    public async Task CheckContainer_ServerDeleted_ServerDoesNotExistAnymore()
    {
        // Act
        await Log("Checking if server still exists...");
        var servers = await _hClient.Server.Get();
        await Log("-> Check completed with existing servers: " + servers.Count);

        //Assert
        Assert.That(servers, Is.Empty);
        await Log("Test completed.");
    }
}
