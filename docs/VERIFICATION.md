# Weather location and forecast verification

Date: 1 November 2026. The existing Desktop project and Docker deployment now support a configurable weather location. Existing user, request and attachment record counts were compared before/after deployment and were unchanged. No schema migration or real device-location transmission was needed for this update.

- Builds: frontend TypeScript/Vite build and both Docker image builds pass. The final frontend refinement was also rebuilt successfully in Docker.
- Automated suite: 115 passed, 0 failed, 0 skipped in Linux with PostgreSQL 16: 65 unit cases and 50 HTTP integration cases. Latest TRX: docs/evidence/weather/weather.trx. All previous account/deletion/CRUD/upload tests remain included.
- New coverage: finite/range coordinate validation, rounded selected-coordinate upstream URL, fixed provider hosts, readable WMO condition labels, malformed current/forecast rejection, escaped city query, valid/empty/malformed city responses, authenticated location search, and invalid HTTP coordinate/query rejection before network calls.
- Coverlet: 2561/2615 instrumented lines (97.93%), branch coverage 71.98%. Includes generated migrations/startup. Raw XML: docs/evidence/weather/coverage.cobertura.xml.
- Live provider check: city search for Legazpi, Philippines returned real matching locations. Selected coordinates 13.14/123.74 returned current conditions and three daily forecasts; example current temperature was 29.7 C at the check. Open-Meteo docs were checked for variables, forecast parameters, WMO mapping and geocoding behavior.
- Browser preview: signed into an isolated synthetic account; opened Settings; searched and selected Legazpi; saw the preview weather/three-day forecast update; navigated to Overview and confirmed the same city; reloaded and confirmed the saved city remained; exercised manual Refresh. Screenshots are in docs/evidence/weather.
- Device location: implemented as a single explicit navigator.geolocation.getCurrentPosition call with browser permission, timeout and denied/unavailable fallbacks. Coordinates are rounded to two decimal places before saving and API transmission. Real-user location permission was deliberately not granted during automated/browser verification; actual permission/device accuracy must be checked by the user when pressing the button.
- Automatic refresh: configured every 15 minutes while visible, plus stale-page refresh on visibility return and manual refresh. The browser verification did not wait through a full 15-minute interval. Data are model-based provider updates, not continuous live GPS or second-by-second sensor readings.
- Preference: browser localStorage uses versioned per-account keys with value validation, cross-tab change handling, default Naga location and reset option. No location is added to the application database. The location selection does not synchronize between devices.
- Live deployment: UI and API health return HTTP 200 after the update. Existing account credentials, data volumes and service records were preserved.
- Secret scan: configured pattern scan reports no findings. .env, trusted CA bundles and build/test caches are excluded from deliverables.

The previous 86-test pages/account-deletion evidence remains in docs/evidence/navigation. Device permission denial/approval, device accuracy, and the full timer interval are explicit verification limits. Public deployment and GitHub CI status remain unverified. Weather and geocoding depend on network/provider availability and retain fallback messages.

Provider references: https://open-meteo.com/en/docs and https://open-meteo.com/en/docs/geocoding-api .


## Clean workspace and GitHub backup verification — 1 November 2026

- User-requested live cleanup: zero service requests, categories, attachments, account deletion requests and Staff accounts. One existing Admin account remains. Related comments and audit records were cleared. Upload storage was emptied.
- Removed automatic category seeding from normal startup. Explicit demo seeding creates synthetic categories and requests only when requested. Confirmed categories remain empty after API restart.
- Linux/PostgreSQL suite: 116 passed, zero failed/skipped. Includes a new isolated-database regression test for empty startup and explicit, idempotent demo seeding. Evidence: docs/evidence/clean-backup/accounts.trx.
- API Docker rebuild and live Docker health checks passed. Local secrets, trust certificates, database storage and uploads are excluded from Git. Pattern-based source secret scan passed.
- Earlier screenshots and test reports use synthetic preview data; they document previous verification and do not represent the current empty live database.
