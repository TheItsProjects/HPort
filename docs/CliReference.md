# CLI Reference

The HPort CLI is the primary way to interact with the HPort system. It follows a standard verb-noun syntax.

**Usage**: `hport [command] [subcommand] [options]`

## Global Options

| Option         | Description                                       |
|:---------------|:--------------------------------------------------|
| `--help`, `-?` | Shows help and usage information for the command. |
| `--version`    | Shows the current version of the tool.            |

---

## `container`

Manages Docker containers and their underlying Hetzner servers.

### `create`

Creates a new container. If the specified server requirements cannot be met by an existing managed server, or if `--unique` is specified, a new Hetzner Cloud server will be provisioned.

**Usage**:
```bash
hport container create --name <name> --compose <path> --env <path> [options]
```

**Options**:

- `--name <name>` **(Required)**: The name of the container/application.
- `--compose <path>` **(Required)**: Path to the `docker-compose.yml` file.
- `--env <path>` **(Required)**: Path to the `.env` file containing environment variables for the compose file.
- `--type <type>`: Hetzner server type (e.g., `Cx23`, `Cpx12`, `Cax11`). *Default: `Cx22`*.
- `--datacenter <loc>`: Datacenter location (e.g., `Nbg` (Nuremberg), `Hel` (Helsinki), `Fsn` (Falkenstein)).
  *Default: `Nbg`*.
- `--is-unique`: If present, ensures the container is deployed to a brand new server, even if others are available.
- `--ssh-key <id>`: The numeric ID of an SSH key in your Hetzner project. This key will be added to the server's root user.
- `--internal-network <id>`: The numeric ID of a Hetzner Cloud Private Network to attach the server to.

### `list`

Lists all containers currently managed by HPort.

**Usage**:
```bash
hport container list [options]
```

**Options**:
- `--server <name>`: Filter the list to only show containers on a specific server.

### `delete`

Deletes a container. By default, if the container is the last one on a server, the server itself is also deleted to save costs.

**Usage**:
```bash
hport container delete --name <name> --server <server-name> [options]
```

**Options**:

- `--name <name>` **(Required)**: The name of the container to delete.
- `--server <name>` **(Required)**: The name of the server where the container is running (retrievable via `list`).
- `--delete-server`: Whether to delete the server if it becomes empty. Defaults to `true`.

### `execute`

Executes a shell command inside a running container's service.

**Usage**:
```bash
hport container execute --name <name> --server <server-name> --service <service> --command <cmd>
```

**Options**:

- `--name <name>` **(Required)**: The name of the container.
- `--server <name>` **(Required)**: The server name.
- `--service <name>` **(Required)**: The service name as defined in your `docker-compose.yml` (e.g., `web`, `db`).
- `--command <cmd>` **(Required)**: The command to run (e.g., `ls -la`, `rails console`).

---

## Examples

**1. Deploy a simple web app:**
```bash
hport container create \
  --name my-website \
  --compose ./deploy/docker-compose.yml \
  --env ./deploy/.env \
  --datacenter hel
```

**2. List all containers:**
```bash
hport container list
```

**3. Run a database migration inside a container:**
```bash
hport container execute \
  --name my-website \
  --server my-website-server \
  --service backend \
  --command "dotnet ef database update"
```

**4. Delete an app (and its server):**
```bash
hport container delete \
  --name my-website \
  --server my-website-server
```
