# Guideline evidence matrix (29 October 2026)

| Requirement | Package evidence | Status |
|---|---|---|
| React/TypeScript, Axios, .NET 9 | frontend, backend | Production UI build and backend build pass |
| PostgreSQL, EF Core, 5+ entities | Six domain entities, Identity, InitialCreate | Initial migration applied to empty PostgreSQL by HTTP test startup |
| Two CRUD entities | Requests and categories | Authenticated database CRUD test passes |
| External API | WeatherService | Success, malformed JSON and upstream failure unit cases pass; live response still environment dependent |
| Two roles and policy | Identity/JWT, CanManageRequests | 401 and 403 tests pass |
| Upload and chart | Signature/type/size limits, private download, DB dashboard | HTTP upload/download/dashboard tests pass; visual UI check reported separately |
| 20 unit / 10 integration | xUnit and WebApplicationFactory, real PostgreSQL | See VERIFICATION.md and full.trx for latest counts |
| Docker | API/UI Dockerfiles, PostgreSQL Compose | Both images and fresh-copy empty-database Compose startup pass with optional trusted CA override |
| CI | Build/test/coverage and Docker smoke jobs | Authored; no remote GitHub run or badge verified |
| Documentation | Updated PDF + IMPLEMENTATION.md + README | Describes actual source and limitations; team evidence still needed |
| Deployment | Container deployment instructions | No public URL or hosting verification |
| Team/process | TEAM_RECORDS.md, PROJECT_PLAN.md, DEFENSE_GUIDE.md | Templates, not actual sprint/contribution/approval evidence |

Not yet submission-complete: instructor-approved proposal and midterm, actual four sprints, per-member commits/reviewed PRs, protected main, release v1.0, public deployment, recorded walkthrough, signed contribution matrix and individual defense. Do not replace missing evidence with fictional records.

Account update: public Admin bootstrap is capped at two with concurrent-request enforcement. Admin-only Staff management and IsActive migration are implemented. Latest 73-test Linux run and browser evidence are in VERIFICATION.md.

November update: separate sidebar pages, browser-persistent light/dark theme, Settings, password-confirmed Admin self-deletion with last-active-Admin protection, and Admin-reviewed Staff deletion requests are implemented. Latest Linux test run: 86 passing cases. See VERIFICATION.md and PAGES_AND_SETTINGS.md.
