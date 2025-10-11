using HetznerCloudApi;
using ItsNameless.HPort.Models;
using ItsNameless.HPort.Test.Utils;

namespace ItsNameless.HPort.IntegrationTest;

[TestFixture]
[TestOf(typeof(HPort))]
public class TestBase
{
    private string _token;
    private Fakers _fakers;
    protected TestConfig _testConfig;
    protected HPort _sut;
    protected HetznerCloudClient _hClient;

    public record TestConfig
    {
        public readonly string ComposeFileName = "compose.yml";
        public readonly string EnvFileName = "env";

        public readonly string ComposeFileContent =
            """
            version: '3.8'
            services:
              web:
                image: nginx:latest
                ports:
                  - "80:80"
                environment:
                  - NGINX_HOST=${NGINX_HOST}
                  - NGINX_PORT=${NGINX_PORT}
            """;

        public readonly string EnvFileContent =
            """
            NGINX_HOST=localhost
            NGINX_PORT=80
            """;

        public readonly PortContainer Container;

        public TestConfig(Fakers fakers)
        {
            Container = fakers.PortContainerFaker.Generate();

            // Ensure cheap server type for testing
            Container.Server.Datacenter = PortDatacenter.Nbg1;
            Container.Server.Type = PortServerType.Cx22;
        }
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fakers = new Fakers();
        _testConfig = new TestConfig(_fakers);

        // Setup Hetzner API token
        await Log("Setting up Hetzner API token...");

        var hetznerTokenFile = File.Exists("HETZNER_TOKEN");
        if (!hetznerTokenFile)
        {
            throw new ArgumentException(
                "HETZNER_TOKEN file not found. Create a file named HETZNER_TOKEN containing your Hetzner API token."
            );
        }

        _token =
            (await File.ReadAllTextAsync("HETZNER_TOKEN")).Trim();
        if (string.IsNullOrWhiteSpace(_token))
        {
            throw new ArgumentException(
                "HETZNER_TOKEN not set write your API token in the file called HETZNER_TOKEN."
            );
        }

        await Log("-> Hetzner API token set.");

        // Ensure Hetzner project is empty
        await Log("Ensuring Hetzner project is empty...");

        _hClient = new HetznerCloudClient(_token);

        foreach (var server in await _hClient.Server.Get())
        {
            await Log($"-> Deleting server {server.Name} ({server.Id})...");
            await _hClient.Server.Delete(server);
        }

        await Log("-> Hetzner project is empty.");

        // Ensure no working files exist
        await Log("Ensuring no working files exist...");

        await DeleteWorkingFile("serverStates.json");
        await DeleteWorkingFile(_testConfig.ComposeFileName);
        await DeleteWorkingFile(_testConfig.EnvFileName);

        await Log("-> No working files exist.");

        // Initialize HPort
        _sut = await HPort.WithDefaults(_token, "serverStates.json");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        // Cleanup working files
        await Log("Ensuring no working files exist...");

        await DeleteWorkingFile("serverStates.json");
        await DeleteWorkingFile(_testConfig.ComposeFileName);
        await DeleteWorkingFile(_testConfig.EnvFileName);

        await Log("-> No working files exist.");

        // Ensure Hetzner project is empty
        await Log("Ensuring Hetzner project is empty...");

        try
        {
            foreach (var server in await _hClient.Server.Get())
            {
                await Log($"-> Deleting server {server.Name} ({server.Id})...");
                await _hClient.Server.Delete(server);
            }
        }
        catch (Exception e)
        {
            await Log(
                $"There was an error during cleanup: {e.Message}. " +
                $"Ensure to manually check the Hetzner project for any remaining servers."
            );
            return;
        }

        await Log("-> Hetzner project is empty.");
    }

    private async Task DeleteWorkingFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            await Log($"-> Deleting existing {fileName} file...");
            File.Delete(fileName);
        }
    }

    protected async Task Log(string message)
    {
        await TestContext.Out.WriteLineAsync(message);
    }
}
