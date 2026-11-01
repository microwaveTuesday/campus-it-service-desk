# Campus IT Service Desk

Updated 1 November 2026. Read docs/VERIFICATION.md for measured results and remaining submission evidence. This package uses synthetic data only.

## Find your way around

Use the sidebar for Operations overview, Service requests, Manage categories (Admin only), Staff management (Admin only), and Settings. On a phone, open the navigation menu at the top left. Light/dark appearance is saved in your browser. See [Pages, appearance and account deletion](docs/PAGES_AND_SETTINGS.md) for the new workflows and manual test checklist.

Admins may delete their own account from Settings after confirming their password, provided another active Admin remains. Staff request account deletion from Settings; Admins review it in Staff management. Service requests and attached files remain after account deletion.

## Run on Windows

**Start here: [How to run the system](RUN_SYSTEM.md).** The guide includes first-time setup, everyday startup, stopping, and the Docker engine error with troubleshooting steps.

1. Open **Docker Desktop** and wait for **Engine running**.
2. Open this project in VS Code and choose **Terminal > New Terminal**.
3. If .env is missing, run `.\scripts\initialize.ps1` once. Keep an existing .env file.
4. Start the existing system:

```powershell
docker compose -p campusitcapstone up -d
```

For a first build or after code changes, use `docker compose -p campusitcapstone up --build -d`.

Open http://localhost:3000. API documentation: http://localhost:8080/swagger; health: http://localhost:8080/health. Migrations apply automatically on startup.

**If you see `failed to connect to the docker API` or `dockerDesktopLinuxEngine`:** open Docker Desktop, wait for its engine to run, then retry. See [the startup troubleshooting guide](RUN_SYSTEM.md#error-failed-to-connect-to-the-docker-api).

Normal setup: register the first Admin in the UI, sign in, then create Staff through the Admin-only Staff management panel. A second Admin may register until both slots are filled. Sign out always returns to login. Staff cannot create accounts or access Staff management.

Optional operator provisioning also respects the same two-Admin limit. Set operator values only in your current terminal:

```powershell
$env:ADMIN_EMAIL='admin@example.test'
$env:ADMIN_PASSWORD=Read-Host 'Synthetic admin password (10+ characters, mixed case, digit, symbol)'
docker compose -p campusitcapstone run --rm -e ADMIN_EMAIL -e ADMIN_PASSWORD api --provision-admin
Remove-Item Env:ADMIN_PASSWORD
```

Existing accounts may be promoted without resetting their password, provided fewer than two Admins exist. Provisioning exits without starting an extra web server. To insert optional synthetic demonstration requests:

```powershell
docker compose -p campusitcapstone run --rm api --seed-demo
```

Requests and categories support full CRUD; Staff creates/edits requests, Admin also deletes requests, manages categories and creates/edits/enables/disables Staff accounts. Disabled accounts cannot sign in and their existing tokens are rejected immediately. Attachments are available from each ticket; downloads require authentication. Dashboard counts come from PostgreSQL. Weather uses Open-Meteo with a four-second timeout and fallback.

## Tests and local development
Requires .NET SDK 9 and Node 22 or newer. Restore the pinned EF tool with `dotnet tool restore`. To expose only the disposable verification database:

```powershell
docker compose -f docker-compose.yml -f docker-compose.test.yml up -d db
```

Set `ConnectionStrings__Default` to the database on localhost:55432 using your .env database name/user/password, and set `Jwt__Key` to your .env key. Set `Jwt__Issuer=CampusIT` and `Jwt__Audience=CampusITUsers` using environment variable names with double underscores. Then run:

```powershell
dotnet test tests/CampusIT.Tests.csproj --collect:'XPlat Code Coverage' --logger trx
npm --prefix frontend ci
npm --prefix frontend run build
```

Tests use real PostgreSQL and create synthetic accounts. Account-registration tests create and drop their own uniquely named databases, so the database user needs permission to CREATE DATABASE. Use a disposable database, never production. The CI build runs the full suite with coverage, builds the frontend using its lockfile, and has a separate Docker Compose smoke job. GitHub passing status and repository badge require an actual GitHub repository; none is configured in the supplied project.

For local API development use `dotnet run --project backend`; frontend `npm --prefix frontend run dev`. The Vite development proxy targets http://localhost:8080/api; set VITE_API_URL for a different address. Database upgrades: `dotnet tool run dotnet-ef database update --project backend`. Migrations are committed; do not regenerate InitialCreate.

## Deployment and evidence
Deploy the API, nginx frontend, PostgreSQL and persistent uploads; use HTTPS, trusted staff enrollment, backups, restricted network access and real secrets. Public hosting, domain, TLS, GitHub branch protection, CI badge, release tag, proposal approval, actual sprint/contribution records and the team recording are not fabricated by this package. Complete docs/TEAM_RECORDS.md using real evidence. No public deployment URL is claimed.

Stopping: `docker compose -p campusitcapstone down` retains data. Restart with `docker compose -p campusitcapstone up -d`. Volume deletion permanently removes data. A sleeping hosted service needs its provider-specific wake-up procedure recorded after deployment.

## Credits and AI use
React, TypeScript, Axios, Chart.js/react-chartjs-2, Vite, ASP.NET Core Identity, EF Core, Npgsql, PostgreSQL, nginx, xUnit, coverlet, Docker and Open-Meteo (https://open-meteo.com). OpenAI ChatGPT/Codex assisted the starter, implementation repair, tests and documentation; verification results are recorded separately. Team members must review and understand all submitted code.


## Optional certificate trust for inspected networks
If npm in a Docker build reports UNABLE_TO_VERIFY_LEAF_SIGNATURE while Windows succeeds, use only certificates already trusted by your organization. On Windows, `./scripts/export-build-ca.ps1` exports public CA certificates (no private keys). Set `$env:BUILD_CA_FILE=(Resolve-Path frontend/build-ca.pem).Path`, then run `docker compose -p campusitcapstone -f docker-compose.yml -f docker-compose.trust.yml up --build -d`. BuildKit mounts the bundle for dependency restoration; certificate verification stays enabled. The public CA bundle is ignored by Git and is not shipped in this package. Ordinary networks do not need this override.

## Account setup update
See docs/ACCOUNT_SETUP.md for the two-Admin signup flow and Staff management. StaffAccountStatus adds an IsActive column with default true, preserving existing accounts. Admin counting and assignment share a PostgreSQL transaction lock, including the operator command. UI hiding is backed by server-side 409/401/403 checks.

If Windows Application Control blocks a locally compiled test assembly, use the included Dockerfile.tests to execute the suite in Linux. No Windows security settings need to be disabled. Build with `docker build -f Dockerfile.tests -t campusit-tests .` (add the trusted CA secret on inspected networks). Run the image with disposable PostgreSQL connection and JWT environment variables and mount a folder at /results for TRX and coverage output. Do not run integration tests against your application database.

## Weather location and forecast
In Settings > Weather location, select Use my current location (with browser permission) or search for a city and choose a result. Current conditions and a three-day forecast appear automatically in Settings and the overview. Weather refreshes every 15 minutes while the page is visible, with a manual Refresh button. The location selection is saved separately for each account on this browser; device coordinates are rounded before saving/transmission. See [Weather location guide](docs/WEATHER_LOCATION.md) for privacy, browser-permission and troubleshooting notes.

## Clean startup and source backup
Normal startup creates the required roles and applies migrations, but does not insert categories, service requests or Staff accounts. Admins add categories and create Staff accounts through the interface. Optional `--seed-demo` explicitly inserts synthetic categories/requests; do not run it when you want the lists to stay empty.

GitHub stores the application source, migrations, instructions and synthetic verification evidence. It does not store live PostgreSQL records, Admin passwords, uploaded files, .env secrets, trusted CA bundles or generated build caches. Keep Docker volumes for your local Admin accounts. For a fresh clone, generate .env and register your own Admin accounts. A GitHub push backs up committed source; it does not automatically back up future uncommitted changes or live data.
