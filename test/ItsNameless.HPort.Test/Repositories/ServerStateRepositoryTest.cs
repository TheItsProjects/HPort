using System.IO.Abstractions.TestingHelpers;
using ItsNameless.HPort.Models;
using ItsNameless.HPort.Repositories;
using ItsNameless.HPort.Test.Utils;

namespace ItsNameless.HPort.Test.Repositories;

[TestFixture]
[TestOf(typeof(ServerStateRepository))]
public class ServerStateRepositoryTest
{
    private Fakers _fakers;
    private MockFileSystem _fileSystem;
    private ServerStateRepository _sut;
    private const string FilePath = "serverStates.json";

    [SetUp]
    public void Setup()
    {
        _fakers = new Fakers();
        _fileSystem = new MockFileSystem();
        _sut = new ServerStateRepository(FilePath, _fileSystem);
    }

    [Test]
    public async Task AddAsync_AddsState()
    {
        // Arrange
        var state =
            new ServerState
            {
                Name = _fakers.Faker.Internet.DomainName(),
                Id = _fakers.Faker.Random.Long(),
                UserPassword = _fakers.Faker.Internet.Password()
            };

        // Act
        await _sut.AddAsync(state);

        // Assert
        var savedState = await _sut.GetAsync(state.Name);
        Assert.That(savedState, Is.Not.Null);
        Assert.That(savedState!.Name, Is.EqualTo(state.Name));
        Assert.That(savedState.Id, Is.EqualTo(state.Id));
        Assert.That(_fileSystem.File.Exists(FilePath), Is.True);
    }

    [Test]
    public async Task AddAsync_ThrowsIfAlreadyExists()
    {
        // Arrange
        var state =
            new ServerState
            {
                Name = _fakers.Faker.Internet.DomainName(),
                Id = _fakers.Faker.Random.Long(),
                UserPassword = _fakers.Faker.Internet.Password()
            };
        await _sut.AddAsync(state);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _sut.AddAsync(state)
        );
    }

    [Test]
    public async Task RemoveAsync_RemovesState()
    {
        // Arrange
        var state =
            new ServerState
            {
                Name = _fakers.Faker.Internet.DomainName(),
                Id = _fakers.Faker.Random.Long(),
                UserPassword = _fakers.Faker.Internet.Password()
            };
        await _sut.AddAsync(state);

        // Act
        await _sut.RemoveAsync(state.Name);

        // Assert
        var result = await _sut.GetAsync(state.Name);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task PruneAsync_RemovesInactiveStates()
    {
        // Arrange
        var state1 =
            new ServerState { Name = "S1", Id = 1, UserPassword = "P1" };
        var state2 =
            new ServerState { Name = "S2", Id = 2, UserPassword = "P2" };
        await _sut.AddAsync(state1);
        await _sut.AddAsync(state2);

        // Act
        await _sut.PruneAsync(new[] { 1L }); // Keep 1, Remove 2

        // Assert
        var result1 = await _sut.GetAsync("S1");
        var result2 = await _sut.GetAsync("S2");
        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllStates()
    {
        // Arrange
        var state1 =
            new ServerState { Name = "S1", Id = 1, UserPassword = "P1" };
        var state2 =
            new ServerState { Name = "S2", Id = 2, UserPassword = "P2" };
        await _sut.AddAsync(state1);
        await _sut.AddAsync(state2);

        // Act
        var allStates = await _sut.GetAllAsync();

        // Assert
        Assert.That(allStates, Has.Count.EqualTo(2));
        Assert.That(allStates.Select(s => s.Name), Contains.Item("S1"));
        Assert.That(allStates.Select(s => s.Name), Contains.Item("S2"));
    }
}
