# Submission checklist and remaining evidence

Read this first. The kit prepares source, documentation, a proposal draft, presentation and demonstration aids. It does not replace the real recorded walkthrough, public hosting, student signatures or actual process evidence.

| Deliverable | Action before sending |
|---|---|
| Final PDF | Enter team identity in editable DOCX. Rename to YourTeamName_ProjectDocumentation.pdf and keep the same final copy in repository /docs. |
| GitHub source | Use https://github.com/microwaveTuesday/campus-it-service-desk. The kit contains a clean source ZIP. Check the final commit and CI after updating documents. |
| Public app URL | Deploy and test the actual application; localhost is not sufficient. Record provider-specific wake steps. |
| Backup video | Record a genuine walkthrough no longer than 10 minutes using the supplied script. Upload it and verify access. |
| Team and Agile records | Complete real names, roles, signatures, board/sprint evidence and per-member commit/PR references. |
| Proposal and midterm | Attach actual approved proposal and Week 9 staging/checkpoint evidence. Drafts do not prove approval. |
| Git freeze | Protect main and create v1.0 on the actual freeze commit/date after review. Do not fabricate past activity. |

The guidelines specify a 15-minute team presentation, 10-minute team Q&A and 10-minute individual defense. Final submission is a complete PDF plus repository and deployed URL, with the backup recording. Follow the instructor portal format for uploads.

# Safe demo setup

Use a separate installation for rehearsal so the cleared working system and its real Admin account remain untouched. Copy/extract CampusITCapstone_Source.zip into a separate folder, create its own .env and start it when the normal application is stopped to avoid port conflicts.

.\scripts\initialize.ps1
docker compose -p campusit-demo up --build -d

The different project name creates separate PostgreSQL and uploads volumes. Ports remain 3000 and 8080, so two installations cannot run on those ports at once. Start Docker Desktop first. Register synthetic Admin/Staff and create only synthetic support records. Do not display real emails, passwords, tokens or personal files in a recording.

| Synthetic fixture | Value |
|---|---|
| Category | Hardware Demo |
| Requester | Demo Requester |
| Issue | Classroom projector does not display an image |
| Location | Demo Building Room 101 |
| Staff | Demo Technician with an example.test email |
| Attachment | A synthetic PNG screenshot or this kit's Demo_Attachment.pdf |

Choose strong passwords privately; no passwords are provided in the kit. To keep your main system empty, do not seed it for the presentation. The optional --seed-demo command is only for a chosen demo installation.

# Fifteen minute presentation plan

| Time | Presenter content |
|---|---|
| 00:00-01:30 | Problem, target users, expected community benefit and SDG rationale. |
| 01:30-04:00 | Scope, architecture and normalized relationships. Explain Axios/API/EF/PostgreSQL flow. |
| 04:00-09:00 | Live demo: Admin category and Staff creation, Staff request create/edit, attachment, overview. |
| 09:00-11:00 | Admin versus Staff permissions, account deletion safeguards and settings/weather. |
| 11:00-13:00 | Tests, coverage, root-cause example and successful CI evidence. |
| 13:00-15:00 | Deployment status, limitations, actual team ownership and next steps. |

Use the presentation speaker notes as a guide, not a memorized claim of work. Assign speaking parts to actual members according to their genuine contributions. Keep credentials off screen when switching roles. Keep the demo browser ready before the session.

During the 10-minute Q&A, show exact source or test references when asked. If the public host is unavailable, use the real recording and explain the failure; do not present an old screenshot as a current running result.

# Backup walkthrough recording script

Target length 8 minutes 45 seconds. Record the running application with your normal screen recorder. This is a script, not a completed video.

| Time | Action and narration |
|---|---|
| 00:00-00:45 | State project title, problem, Admin/Staff roles and that all displayed data are synthetic. |
| 00:45-01:35 | Show Admin overview and separate pages. Create Hardware Demo category and Demo Technician. |
| 01:35-03:10 | Sign in as synthetic Staff. Create the projector issue, read it and change Pending to In Progress. Explain that the API persists the record in PostgreSQL. |
| 03:10-04:00 | Upload Demo_Attachment.pdf and download it. Mention the file/type/size validation. |
| 04:00-04:50 | Resolve the request and show updated database-backed overview counts/chart. |
| 04:50-05:35 | Open Settings, choose appearance and a city. Explain external API fallback and approximate location. |
| 05:35-06:35 | Show Staff has no management page. Submit a synthetic deletion request if safe, then review as Admin. Do not remove the last Admin. |
| 06:35-07:30 | Show the real successful CI run, test results and one root-cause fix. |
| 07:30-08:45 | Show public URL if deployed; explain setup/wake instructions, limitations and genuine team contributions. |

Watch the saved video, check audio/legibility and total duration, and open its shared link from another permitted session. Put the actual video URL in Submission_Links.md. Never include local secrets or personal data.

# Individual defense questions and answer cues

| Question | Source-based answer cue |
|---|---|
| How does a ticket reach PostgreSQL? | Axios sends DTO + JWT; middleware authorizes; RequestsController validates; ServiceRequestService uses AppDbContext; EF persists; UI reloads. |
| Why both a role and a policy? | Admin/Staff are roles. CanManageRequests allows either for create/update and attachments; Admin-only rules guard deletion and management. |
| Where are passwords kept? | Identity hashes them in AspNetUsers. JWT key and database password come from environment, excluded from Git. |
| How is the third Admin rejected? | AccountService uses a transaction/advisory lock shared with other Admin lifecycle operations and enforces the two-Admin cap. |
| Why can't the last Admin delete? | The account controller counts active Admins inside the shared transaction lock before deletion. |
| What does 3NF mean here? | Separate category/user/file relations prevent repeating unrelated details; foreign keys enforce relationships. |
| What is an integration test? | An HTTP request through real middleware/controllers against real PostgreSQL using WebApplicationFactory. |
| What is the external API fallback? | Fixed Open-Meteo hosts, a four-second timeout, validated inputs/responses and readable unavailable state. |
| What did you personally contribute? | Show your real issue, commits, reviewed PRs, tests and a debugging example. Do not claim generated scaffolding as personal work. |

For a 10-minute individual defense, prepare 2 minutes on your actual module, 2 on data/security decisions, 2 on tests and a defect, and the remaining time for follow-up questions.

# Final verification and handoff

1. Review team identity, add missing section/student numbers, and complete the AI declaration and signed contribution matrix. Remove no limitations unless verified evidence resolves them. Re-export the edited DOCX to the final required PDF filename.

2. Add real board links, four sprint records, approved proposal and checkpoint evidence. Verify required per-member history with the instructor. One initial backup commit and generated templates do not satisfy those requirements.

3. Deploy, complete provider wake instructions and test the hosted browser-to-database workflows. Record the real walkthrough and its link.

4. Save the final PDF in project /docs, commit and push the updated source/documentation, check CI, then use the actual course freeze procedure for v1.0. No release tag was created by this preparation.

5. Submit the documentation PDF, repository URL, public application URL and accessible backup recording. Attach source ZIP and slides only if the portal/instructor asks. Keep the full kit for your own defense preparation.

Verification limits on 23 November: Docker Desktop engine was stopped so no fresh full application test was run. Existing 116-test evidence from 1 November and successful GitHub CI were inspected. The prior empty-workspace reset is recorded history; current database contents were not rechecked during this preparation.
