# Photo preview regression check

This script creates synthetic Admin/Staff accounts, a category, a request and uploaded files in a **fresh disposable stack**. It refuses the live application ports. Never point these checks at your real database.

The separate UI/API ports are localhost:4310 and localhost:4810. The live app uses 3000/8080 and is unaffected.

From the project root, build the current API/frontend images if necessary (see the root README for the optional trusted-CA build override):

```powershell
docker compose -p campusitcapstone build
$env:PREVIEW_TEST_DB_PASSWORD = [guid]::NewGuid().ToString('N')
$env:PREVIEW_TEST_JWT_KEY = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
docker compose -p campusit-preview-verify -f tests/browser/compose.yml up -d --wait
npm --prefix tests/browser ci
cd tests/browser
npx playwright install chromium
npm test
cd ../..
docker compose -p campusit-preview-verify -f tests/browser/compose.yml down -v
```

Run the final cleanup even if a test fails. Before retrying the suite, recreate the disposable stack with fresh storage so its synthetic Admin bootstrap slots are available. Keep the generated environment variables for the cleanup command; they are only disposable fixture secrets.

If Google Chrome is already installed, set `$env:PREVIEW_BROWSER_CHANNEL='chrome'` before `npm test` to use it instead of installing Chromium. Tests launch an isolated browser profile and do not use your personal browser session.

Optional `PREVIEW_EVIDENCE_DIR` saves synthetic desktop/light and mobile/dark screenshots. The script checks protected PNG/JPG rendering, loading, on-demand requests, URL cleanup, recoverable network/type/decode failures, mobile fit, PDF download byte preservation and anonymous 401. Browser page errors fail the run. No passwords or JWTs are printed.
