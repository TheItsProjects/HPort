# Troubleshooting

## Common Issues

### Server creation fails with "Server not ready"

**Symptoms**:
- The CLI hangs for a long time during creation.
- Error message: `Server <name> is not ready: ...`

**Possible Causes**:
- **SSH Key Issue**: If you provided an SSH key ID, ensure the key is valid and added to the project.
- **Network Timeout**: Occasionally, cloud-init scripts take longer than expected.
- **Bad Config**: Errors in your `docker-compose.yml` or `.env` file might prevent the container from starting.

**Debug Steps**:
1. Check the server console in Hetzner Cloud.
2. SSH into the server manually (if you added your SSH key) or via the password in `serverStates.json` (if accessible).
3. Check logs: `/var/log/cloud-init-output.log`.

### Docker Command Fails

**Symptoms**:
- `execute` command returns an error.
- Container status shows "Exited".

**Debug Steps**:
1. Run `hport container execute ... --command "docker logs <container-id>"` (Note: You might need to adjust this to run on the host, which currently requires manual SSH).
2. Verify that the service name matches exactly what is in `docker-compose.yml`.

### "Server with name ... already exists"

**Cause**:
- You are trying to create a container on a server name that HPort thinks is already taken.
- Alternatively, you deleted the `serverStates.json` file, but the server still exists on Hetzner.

**Solution**:
- If the server exists on Hetzner but not in HPort's list: Delete it manually on Hetzner or add it to `serverStates.json` manually (advanced).
- If you want a new server, use a different container name or specify `--unique`.
