namespace ItsNameless.HPort.IntegrationTest;

[TestFixture]
[TestOf(typeof(HPort))]
public class MultipleContainersTest : TestBase
{
    [Test, Order(1),]
    public async Task
        AddFirstContainer_NoServerExists_CreatesServer_AndAddsContainer()
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

    [Test, Order(2),]
    public async Task
        AddSecondContainer_ServerExists_AddsContainer()
    {
        // Act
        await Log("Creating container...");
        var actualContainer =
            await _sut.Container.CreateContainer(
                _otherTestConfig.Container.Name,
                _otherTestConfig.Container.Server.Type,
                _otherTestConfig.Container.Server.Datacenter,
                _otherTestConfig.ComposeFileName,
                _otherTestConfig.EnvFileName,
                uniqueServer: false
            );
        await Log("-> Container created.");

        //Assert
        Assert.That(actualContainer, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                actualContainer.Name,
                Is.EqualTo(_otherTestConfig.Container.Name)
            );
            Assert.That(actualContainer.Server, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                actualContainer.Server.Type,
                Is.EqualTo(_otherTestConfig.Container.Server.Type)
            );
            Assert.That(
                actualContainer.Server.Datacenter,
                Is.EqualTo(_otherTestConfig.Container.Server.Datacenter)
            );
        }

        await Log("Test completed.");
    }


    [Test, Order(11),]
    public async Task
        CheckFirstContainer_RunsCommandInContainer_AndReturnsResult()
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

    [Test, Order(12),]
    public async Task
        CheckSecondContainer_RunsCommandInContainer_AndReturnsResult()
    {
        // Act
        // Wait some time to ensure the container is fully started.
        await Task.Delay(5000);

        await Log("Executing command in container...");
        var result =
            await _sut.Container.ExecuteCommandInContainer(
                _otherTestConfig.Container.Name,
                _otherTestConfig.Container.Server.Name,
                _otherTestConfig.ServiceName,
                "nginx -v"
            );
        await Log("-> Command executed with result: " + result);

        //Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("nginx version: nginx/"));
        await Log("Test completed.");
    }

    [Test, Order(91),]
    public async Task DeleteFirstContainer_DeletesContainer_AndKeepsServer()
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

    [Test, Order(92),]
    public async Task DeleteSecondContainer_DeletesContainer_AndServer()
    {
        // Act
        await Log("Deleting container...");
        var actualContainer =
            await _sut.Container.DeleteContainer(
                _otherTestConfig.Container.Name,
                _otherTestConfig.Container.Server.Name,
                true
            );
        await Log("-> Container deleted.");

        //Assert
        Assert.That(actualContainer, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                actualContainer.Name,
                Is.EqualTo(_otherTestConfig.Container.Name)
            );
            Assert.That(actualContainer.Server, Is.Not.Null);
        }

        Assert.That(
            actualContainer.Server.Name,
            Is.EqualTo(_otherTestConfig.Container.Server.Name)
        );
        await Log("Test completed.");
    }

    [Test, Order(93),]
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
