using System.Security.Cryptography;
using ItsNameless.HPort.Models;
using static BCrypt.Net.BCrypt;

namespace ItsNameless.HPort.Repositories;

internal partial class ServerRepository
{
    private const long DOCKER_IMAGE_ID = 40093247;
    private const string SERVER_CREATING_STATUS = "initializing";
    private const string SERVER_STARTING_STATUS = "starting";
    private const string SERVER_READY_STATUS = "running";
    private const string INITIALIZED = "INITIALIZED";

    private const string CLOUD_CONFIG_TEMPLATE =
        "#cloud-config" + "\nusers:" +
        "\n  - name: {USER_NAME}" +
        "\n    sudo: ['ALL=(ALL) NOPASSWD:ALL']" +
        "\n    shell: /bin/bash" +
        "\n    create_home: true" +
        "\n    groups: docker" +
        "\n    passwd: {USER_PASSWORD}" +
        "\n    lock_passwd: false" +
        "\npackage_update: true" +
        "\npackage_upgrade: true" +
        "\ncloud_config_modules:" +
        "\n  - runcmd" +
        "\ncloud_final_modules:" +
        "\n  - scripts-user" +
        "\nwrite_files:" +
        "\n  - path: /home/{USER_NAME}/{CONTAINER_NAME}/docker-compose.yml" +
        "\n    content: |" +
        "\n      {COMPOSE_CONTENT}" +
        "\n  - path: /home/{USER_NAME}/{CONTAINER_NAME}/.env" +
        "\n    content: |" +
        "\n      {ENV_CONTENT}" +
        "\nruncmd:" +
        "\n  - chown {USER_NAME}:{USER_NAME} -R /home/{USER_NAME}" +
        "\n  - docker compose -f /home/{USER_NAME}/{CONTAINER_NAME}/docker-compose.yml up -d" +
        "\n  - touch /home/{USER_NAME}/{CONTAINER_NAME}/{INITIALIZED}";

    private static string GenerateRandomPassword(int length)
    {
        const string chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var password = new char[length];

        for (int i = 0; i < length; i++)
        {
            password[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        return new string(password);
    }

    private string GetCloudConfig(
        string userPassword,
        string containerName,
        string composeContent,
        string envContent)
    {
        return CLOUD_CONFIG_TEMPLATE.Replace("{USER_NAME}", PortServer.User)
            .Replace("{USER_PASSWORD}", HashPassword(userPassword))
            .Replace("{CONTAINER_NAME}", containerName)
            .Replace("{COMPOSE_CONTENT}", composeContent)
            .Replace("{ENV_CONTENT}", envContent)
            .Replace("{INITIALIZED}", INITIALIZED);
    }
}
