using System.IO.Abstractions.TestingHelpers;
using ItsNameless.HPort.Repositories;
using ItsNameless.HPort.Services;
using ItsNameless.HPort.Test.Utils;
using ItsNameless.HPort.Test.Utils.Extensions;
using NSubstitute;

namespace ItsNameless.HPort.Test.Services;

[TestFixture]
[TestOf(typeof(ContainerService))]
public class ContainerServiceTest
{
    private Fakers _fakers;
    private IContainerRepository _containerRepository;
    private IServerRepository _serverRepository;
    private ContainerService _sut;

    private string _composeFilePath;
    private string _envFilePath;

    [SetUp]
    public void SetUp()
    {
        _fakers = new Fakers();

        _serverRepository = Substitute.For<IServerRepository>();
        _containerRepository = Substitute.For<IContainerRepository>();

        var fileSystem = new MockFileSystem();
        _composeFilePath = "compose.yml";
        _envFilePath = ".env";
        fileSystem.AddFile(_composeFilePath, "COMPOSE");
        fileSystem.AddFile(_envFilePath, "ENV");

        _sut =
            new ContainerService(
                _serverRepository,
                _containerRepository,
                fileSystem
            );
    }

    [Test]
    public async Task
        CreateContainer_ReceivesOptions_CallsRepositories_AndPassesOptions()
    {
        // Arrange
        var expectedContainer = _fakers.PortContainerFaker.Generate();

        _containerRepository
            .CreateContainerOnNewServer(
                null!,
                default,
                default,
                null,
                null!,
                null!,
                null!
            )
            .ReturnsAsyncForAnyArgs(expectedContainer);

        // Act
        var actualContainer =
            await _sut.CreateContainer(
                expectedContainer.Name,
                expectedContainer.Server.Type,
                expectedContainer.Server.Datacenter,
                _composeFilePath,
                _envFilePath
            );

        //Assert
        Assert.That(actualContainer, Is.EqualTo(expectedContainer));
        await _serverRepository.ReceivedWithAnyArgs(1).GetServer(null!);
        await _containerRepository.ReceivedWithAnyArgs(1)
            .CreateContainerOnNewServer(
                null!,
                default,
                default,
                null,
                null!,
                null!,
                null!
            );
    }

    [Test]
    public async Task CreateContainer_ServerExists_AddsContainer()
    {
        // Arrange
        var expectedServer = _fakers.PortServerFaker.Generate();
        var expectedContainer =
            _fakers.PortContainerFaker.Generate() with
            {
                Server = expectedServer
            };

        _serverRepository
            .GetServer(null!)!
            .ReturnsAsyncForAnyArgs(expectedServer);

        _containerRepository
            .CreateContainerOnExistingServer(
                null!,
                null!,
                null!,
                null!
            )
            .ReturnsAsyncForAnyArgs(expectedContainer);

        // Act
        var actualContainer =
            await _sut.CreateContainer(
                expectedContainer.Name,
                expectedContainer.Server.Type,
                expectedContainer.Server.Datacenter,
                _composeFilePath,
                _envFilePath
            );

        //Assert
        Assert.That(actualContainer, Is.EqualTo(expectedContainer));
        await _serverRepository.ReceivedWithAnyArgs(1).GetServer(null!);
        await _containerRepository.ReceivedWithAnyArgs(1)
            .CreateContainerOnExistingServer(
                null!,
                null!,
                null!,
                null!
            );
    }

    [Test]
    public void CreateContainer_ServerExists_ButUniqueGiven_Throws()
    {
        // Arrange
        var expectedContainer = _fakers.PortContainerFaker.Generate();

        _serverRepository
            .GetServer(null!)!
            .ReturnsAsyncForAnyArgs(expectedContainer.Server);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                _sut.CreateContainer(
                    expectedContainer.Name,
                    expectedContainer.Server.Type,
                    expectedContainer.Server.Datacenter,
                    _composeFilePath,
                    _envFilePath,
                    uniqueServer: true
                )
        );
    }

    [Test]
    public async Task CreateContainer_UniqueGiven_CreatesServerWithCorrectName()
    {
        // Arrange
        var expectedContainer = _fakers.PortContainerFaker.Generate();
        var expectedServerName =
            ServerRepository.Name.GetServerName(
                expectedContainer.Name,
                expectedContainer.Server.Type,
                expectedContainer.Server.Datacenter,
                true
            );

        string actualServerName = null!;

        _containerRepository
            .CreateContainerOnNewServer(
                null!,
                default,
                default,
                null,
                null!,
                null!,
                null!
            )
            .ReturnsAsyncForAnyArgs(expectedContainer)
            .AndDoes(c => actualServerName = c.ArgAt<string>(4));

        // Act
        await _sut.CreateContainer(
            expectedContainer.Name,
            expectedContainer.Server.Type,
            expectedContainer.Server.Datacenter,
            _composeFilePath,
            _envFilePath,
            uniqueServer: true
        );

        //Assert
        Assert.That(actualServerName, Is.EqualTo(expectedServerName));
    }

    [Test]
    public async Task
        GetContainers_NoServerGiven_CallsRepository_WithAllContainers()
    {
        // Arrange
        string actualServerName = null!;

        _containerRepository
            .GetAllContainers()
            .ReturnsAsyncForAnyArgs([]);


        // Act
        var actualContainers = await _sut.GetContainers();

        //Assert
        Assert.That(actualContainers, Is.Not.Null);
        Assert.That(actualServerName, Is.Null);
    }

    [Test]
    public async Task GetContainers_ServerGiven_CallsRepository_WithServerName()
    {
        // Arrange
        string expectedServerName = _fakers.Faker.Company.CompanyName();
        string actualServerName = null!;

        _containerRepository
            .GetAllContainers(null!)
            .ReturnsAsyncForAnyArgs([])
            .AndDoes(c => actualServerName = c.Arg<string>());


        // Act
        var actualContainers = await _sut.GetContainers(expectedServerName);

        //Assert
        Assert.That(actualContainers, Is.Not.Null);
        Assert.That(actualServerName, Is.EqualTo(expectedServerName));
    }

    [Test]
    public async Task
        ExecuteCommandInContainer_ReceivesOptions_CallsRepository_AndPassesOptions()
    {
        // Arrange
        var expectedServer = _fakers.PortServerFaker.Generate();
        var expectedContainer = _fakers.PortContainerFaker.Generate();
        var expectedService = _fakers.Faker.Company.CompanyName();
        var expectedCommand = _fakers.Faker.Hacker.Random.String();
        var expectedResult = _fakers.Faker.Hacker.Random.String();
        string actualServerName = null!;
        string actualContainerName = null!;
        string actualServiceName = null!;
        string actualCommand = null!;

        _serverRepository
            .GetServer(null!)!
            .ReturnsAsyncForAnyArgs(expectedServer);

        _containerRepository
            .ExecuteCommandInContainer<string>(
                null!,
                null!,
                null!,
                null!,
                null!
            )
            .ReturnsAsyncForAnyArgs(string.Empty)
            .AndDoes(c => actualContainerName = c.ArgAt<string>(0))
            .AndDoes(c => actualServerName = c.ArgAt<string>(1))
            .AndDoes(c => actualServiceName = c.ArgAt<string>(2))
            .AndDoes(c => actualCommand = c.ArgAt<string>(3));


        // Act
        var actualResult =
            await _sut.ExecuteCommandInContainer(
                expectedContainer.Name,
                expectedServer.Name,
                expectedService,
                expectedCommand
            );

        //Assert
        Assert.That(actualResult, Is.Empty);
        Assert.That(actualContainerName, Is.EqualTo(expectedContainer.Name));
        Assert.That(actualServerName, Is.EqualTo(expectedServer.Name));
        Assert.That(actualServiceName, Is.EqualTo(expectedService));
        Assert.That(actualCommand, Is.EqualTo(expectedCommand));
    }

    [Test]
    public async Task
        DeleteContainer_ReceivesOptions_CallsRepository_AndPassesOptions()
    {
        // Arrange
        var expectedServer = _fakers.PortServerFaker.Generate();
        var expectedContainer = _fakers.PortContainerFaker.Generate();
        var expectedDeleteServerIfEmpty = _fakers.Faker.Random.Bool();
        string actualContainerName = null!;
        string actualServerName = null!;
        bool actualDeleteServerIfEmpty = false;

        _containerRepository
            .DeleteContainer(null!, null!, false)
            .ReturnsAsyncForAnyArgs(expectedContainer)
            .AndDoes(c => actualContainerName = c.ArgAt<string>(0))
            .AndDoes(c => actualServerName = c.ArgAt<string>(1))
            .AndDoes(c => actualDeleteServerIfEmpty = c.ArgAt<bool>(2));

        // Act
        var actualContainer =
            await _sut.DeleteContainer(
                expectedContainer.Name,
                expectedServer.Name,
                expectedDeleteServerIfEmpty
            );

        //Assert
        Assert.That(actualContainer, Is.EqualTo(expectedContainer));
        Assert.That(actualContainerName, Is.EqualTo(expectedContainer.Name));
        Assert.That(actualServerName, Is.EqualTo(expectedServer.Name));
        Assert.That(
            actualDeleteServerIfEmpty,
            Is.EqualTo(expectedDeleteServerIfEmpty)
        );
    }
}
