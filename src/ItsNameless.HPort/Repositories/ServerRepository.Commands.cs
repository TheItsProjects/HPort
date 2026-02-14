using ItsNameless.HPort.Models;

namespace ItsNameless.HPort.Repositories;

internal partial class ServerRepository
{
    /// <summary>
    /// Helper class for generating server commands.
    /// </summary>
    internal static class Commands
    {
        internal static string CreateContainerDirectory(string containerName)
            => $"mkdir /home/{PortServer.User}/{containerName}";

        internal static string CreateComposeFile(
            string containerName,
            string composeContent)
            =>
                $"echo \"{composeContent.Replace("\"", "\\\"").Replace("$", "\\$")}\" > /home/{PortServer.User}/{containerName}/docker-compose.yml";

        internal static string CreateEnvFile(
            string containerName,
            string envContent)
            =>
                $"echo \"{envContent.Replace("\"", "\\\"").Replace("$", "\\$")}\" > /home/{PortServer.User}/{containerName}/.env";

        internal static string StartContainer(string containerName)
            =>
                $"docker compose -f /home/{PortServer.User}/{containerName}/docker-compose.yml up -d";

        internal static string CheckContainerIsRunning(string containerName)
            =>
                $"docker compose -f /home/{PortServer.User}/{containerName}/docker-compose.yml ps --format '{{{{.Names}}}}'";

        internal static IEnumerable<string> CheckCloudInitLogs()
            =>
            [
                "sudo awk '/^Error/ {print; c=5; next} c {print; c--}' /var/log/cloud-init.log",
                "sudo awk '/^Error/ {print; c=5; next} c {print; c--}' /var/log/cloud-init-output.log",
            ];

        internal static string CreateInitializedFile(string containerName)
            => $"touch /home/{PortServer.User}/{containerName}/INITIALIZED";

        internal static string ExecInContainer(
            string containerName,
            string serviceName,
            string command)
            =>
                $"docker compose -f /home/{PortServer.User}/{containerName}/docker-compose.yml exec {serviceName} {command}";

        internal static string StopContainer(string containerName)
            =>
                $"docker compose -f /home/{PortServer.User}/{containerName}/docker-compose.yml down";

        internal static string DeleteContainerDirectory(string containerName)
            => $"rm -rf /home/{PortServer.User}/{containerName}";

        internal static string GetContainers() =>
            $"ls -d /home/{PortServer.User}/*/ 2>/dev/null | cut -d'/' -f4";
    }
}
