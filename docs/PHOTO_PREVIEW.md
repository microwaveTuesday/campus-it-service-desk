# Request attachment photo preview

Added 24 November 2026. In Service requests, open **Files**, then choose **Preview photo** next to a PNG or JPG. The image opens inside the same dialog, fits the screen and can be hidden again. **Download** remains available. Existing uploaded photos work without reuploading. PDFs retain their Download button.

Preview requests use the same authenticated Axios file endpoint as downloads. Only PNG/JPEG response MIME types are rendered. No upload URL or token is exposed through a public file link. Files load on demand, pending requests are cancelled when hidden/closed, and temporary object URLs are released. Network errors, unexpected types and corrupt images show a retry/download message.

Verification: TypeScript/Vite production build and frontend Docker build passed. Ten executable browser checks passed in isolated Chrome with a separate PostgreSQL/API/frontend stack on ports 4810/4310, using synthetic Staff and uploads. No browser page errors were recorded. Tests cover PNG and uppercase-JPG uploads/previews, loading and on-demand fetching, authenticated bytes, hide/close cleanup, network retry, unexpected MIME rejection, corrupt-image recovery, 390px dark-mode layout, unchanged PDF download bytes/name, and anonymous 401. Synthetic screenshots are in docs/evidence/photo-preview. Reproduction instructions and fixtures: tests/browser/README.md.

The feature adds no schema changes and preserves existing application records and attachments. The earlier 116-case .NET evidence remains historical until the next CI run; this change did not rerun that suite locally because backend code is unchanged. The new browser checks are run separately from the existing CI backend/build jobs.
