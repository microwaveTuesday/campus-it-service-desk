# Pages, appearance and account deletion

The sidebar opens Operations overview, Service requests, Manage categories (Admin), Staff management (Admin), and Settings. Each page has its own URL fragment, so browser Back/Forward and bookmarks work. On small screens use the menu button at the top left. Forms open in focused dialogs instead of requiring a long scroll.

Light and dark modes are available in Settings and from the sidebar. The selected mode is saved for this browser and also applies to sign-in. The overview loads counts independently of the weather service. Service requests can be searched and filtered by status; file upload explains accepted formats and limits.

## Account deletion
- Admins delete only their own account from Settings, with their current password and explicit confirmation. The last active Admin cannot delete their account. The server serializes Admin deletion with registration/provisioning so concurrent actions cannot remove the last Admin.
- Deleting an Admin frees one registration slot under the existing two-Admin limit. Trusted operators should manage any reopened slot.
- Staff submit a deletion request from Settings, confirming their password and optionally giving a reason. Accounts remain usable while waiting for approval. Staff can cancel their own pending request.
- Admins review pending requests on Staff management. Decline keeps the Staff account; Approve deletes its sign-in account. Reviews cannot delete Admin accounts or process a cancelled/already-reviewed request.
- Account deletion removes Identity credentials/profile and immediately blocks its token. Existing service requests, requester text and attachments remain as service-desk history, with the deleted owner's link cleared. This does not erase all information ever entered in tickets. Explain retention to real users before collecting personal data.

## API and database
GET /api/account/me returns safe profile/role information and current deletion-request state. DELETE /api/account confirms the Admin's password and protects the last active Admin. POST/DELETE /api/account/deletion-request submits/cancels a Staff request. GET /api/account/deletion-requests and POST /api/account/deletion-requests/{id}/review require Admin.

AccountDeletionRequests migration adds the review table with nullable user/reviewer foreign keys (SET NULL) and a unique filtered index allowing only one Pending request per user. The migration applies automatically at startup; no database reset is needed.

## Manual checks
1. Navigate each sidebar page; verify only that page appears and browser Back works.
2. Switch to dark mode, reload, then return to light mode; confirm preference persists.
3. Search/filter requests and create/edit through the dialog; add/download a supporting file.
4. Check mobile navigation and dialog readability at a narrow screen width.
5. With a disposable database, verify a lone Admin sees the last-Admin notice and cannot delete.
6. With two disposable Admins, verify one may delete itself but the remaining Admin cannot.
7. As Staff, submit a deletion request; verify Pending status and cancellation.
8. As Admin, decline one request, then approve a newly submitted request; verify Staff access ends and tickets remain.

Use synthetic accounts in an isolated test instance for deletion tests, not real team accounts. Latest automated results and screenshots are in docs/evidence/navigation.

## Weather location update
Settings now includes browser-per-account weather location selection, permission-based device location and manual city search. Current conditions and a three-day forecast replace the fixed Naga-only display. See WEATHER_LOCATION.md and the latest VERIFICATION.md.
