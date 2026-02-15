# Configuration

HPort is configured primarily through environment variables. This design choice makes it easy to use in CI/CD pipelines and ensures secrets are not hardcoded in command arguments where possible.

## Environment Variables

### `HPORT_TOKEN` (Required)

This is your Hetzner Cloud API Token. HPort uses this to authenticate against the Hetzner API to create servers, list resources, and manage networks.

*   **Type**: String (Read/Write access recommended)
*   **Example**:
    ```bash
    export HPORT_TOKEN="XyZ1234567890abcdef..."
    ```

### `HPORT_STATE_FILE` (Optional)

HPort maintains a local state file to track servers it has created. This is crucial because it stores the **generated
passwords** for the `deploy` user (which has sudo privileges) on the servers. These passwords are not retrievable from
Hetzner after creation.

*   **Type**: File Path
*   **Default**: `serverStates.json` (in the current working directory)
*   **Example**:
    ```bash
    export HPORT_STATE_FILE="/var/data/hport/servers.json"
    ```

> **Warning**: Keep your `HPORT_STATE_FILE` secure! It contains sensitive credentials (passwords) for your cloud servers.

## State Management

When HPort creates a server, it:
1.  Generates a random secure password.
2.  Provisions the server on Hetzner with this password.
3.  Saves the Server ID, Name, and Password to the `HPORT_STATE_FILE`.

If you lose this file, HPort (and you) will lose the ability to authenticate with the servers programmatically via the generated password.
