using ItsNameless.HPort.Models;

namespace ItsNameless.HPort.Repositories;

internal partial class ServerRepository
{
    /// <summary>
    /// Helper class for generating server names.
    /// </summary>
    internal static class Name
    {
        internal static string GetServerName(
            string containerName,
            PortServerType serverType,
            PortDatacenter datacenter,
            bool uniqueServer = false)
            => uniqueServer
                ? $"{containerName}-{serverType.ToString()}-{datacenter.ToString()}"
                : $"{serverType.ToString()}-{datacenter.ToString()}";
    }
}
