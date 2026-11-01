# How to run Campus IT Service Desk on Windows

## Every time you run the system

1. Open **Docker Desktop** from the Windows Start menu.
2. Wait until Docker Desktop shows **Engine running**. Keep Docker Desktop running while using the system.
3. Open the CampusITCapstone folder in VS Code and choose **Terminal > New Terminal**.
4. Make sure the terminal is in the folder containing docker-compose.yml. For the current local copy:

   ```powershell
   cd "C:\Users\yujin\OneDrive\Desktop\INTEG1 FINAL PROJECT\CampusITCapstone"
   ```

5. Start the system:

   ```powershell
   docker compose -p campusitcapstone up -d
   ```

6. Wait for the containers to start, then open **http://localhost:3000** in your browser. If the page is not ready immediately, wait a few seconds and refresh.
7. Sign in with your existing account. For a new database, choose **Register an Admin**. Public registration accepts only two Admin accounts; Admins create Staff through **Staff management**.

Use the same project name, `campusitcapstone`, for start, stop and troubleshooting commands so the system uses the same database volume and saved accounts.

## First run on a new copy

Complete steps 1-4 above. If the project does not have a .env file, create its local configuration once:

```powershell
.\scripts\initialize.ps1
```

If .env already exists, keep it and skip initialization. Then build and start:

```powershell
docker compose -p campusitcapstone up --build -d
```

The first build downloads dependencies and may take several minutes. Database migrations apply automatically. Docker Desktop with Compose is required; a local .NET or Node installation is not required for this Docker startup method.

## Error: failed to connect to the Docker API

An error mentioning `dockerDesktopLinuxEngine`, `the daemon is running`, or `The system cannot find the file specified` usually means the Docker engine has not started.

1. Open Docker Desktop.
2. Wait for **Engine running**.
3. Return to VS Code and check the engine:

   ```powershell
   docker info --format '{{.ServerVersion}}'
   ```

4. Once a server version is displayed, retry:

   ```powershell
   docker compose -p campusitcapstone up -d
   ```

If Docker Desktop itself reports a WSL or startup error, resolve the message shown in Docker Desktop before retrying. If Docker Desktop remains stuck, quit and reopen it; restart Windows if it requests a restart.

## Build error: certificate verification failed

For this machine's inspected network, use the supplied certificate-trust option when a build reports `UNABLE_TO_VERIFY_LEAF_SIGNATURE` or a certificate-chain error:

```powershell
.\scripts\export-build-ca.ps1
$env:BUILD_CA_FILE = (Resolve-Path frontend/build-ca.pem).Path
docker compose -p campusitcapstone -f docker-compose.yml -f docker-compose.trust.yml up --build -d
```

This uses public certificates already trusted by Windows and keeps certificate verification enabled. For subsequent starts with existing images, use the normal `docker compose -p campusitcapstone up -d` command.

## If the website does not open

Check the containers and recent messages:

```powershell
docker compose -p campusitcapstone ps
docker compose -p campusitcapstone logs --tail 50 api frontend
```

The website is http://localhost:3000. API documentation is http://localhost:8080/swagger. API health is http://localhost:8080/health. If a port is already in use, another copy may be running; inspect `docker ps` before starting a second copy. Share the error output if further help is needed, but do not share .env or passwords.

## Stop the system

From the project terminal:

```powershell
docker compose -p campusitcapstone down
```

This stops the containers and retains saved accounts, requests and attachments in Docker volumes. Avoid adding `-v` unless you intentionally want to delete the saved volumes and reset the database.
