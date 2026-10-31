# Demo plan: VerticalSliceFastEndpoints (repo commit 9a638cc)

Date: 2026-10-31 | Owner: Elmor Cabalfin | Status: Approved by the task brief (single-run production)

## Goal

- **Type:** Full tutorial / portfolio walkthrough (5-15 min type, target 7:30; brief asked for 6-9 min)
- **Target length:** about 7:30
- **Audience:** hiring managers and engineers reviewing a portfolio
- **After watching, they should:** open the GitHub repository and get in touch about software engineering roles that use AI-assisted development
- **Shared on:** portfolio / LinkedIn / repository README (not uploaded by this task)

## Story

| Part | What we say | What we show | Seconds |
|---|---|---|---|
| Problem | Features scattered across layers are hard to find and change | README, stack, feature folders | 60 |
| Solution | Vertical slices + FastEndpoints; shared wiring in Program.cs; audit and soft delete in one place | Real source files | 90 |
| Setup | Prerequisites, config, Migrate/Seed on first run | Captured terminal output | 100 |
| Result | Live API: register, login, JWT, 401/201, validation, JSON/XML/406, Dapper, soft delete | Scalar, real running app | 150 |
| AI workflow | CLAUDE.md, /docs conventions, .claude agents, commands, skills, Stop hook; honest limits | Real repo files | 90 |
| Call to action | Hiring managers: let's talk | End card with repo link | 12 |

## Chapters

| # | Title | Journey | Start (approx.) |
|---|---|---|---|
| 1 | Introduction | J0 | 0:00 |
| 2 | Technology stack | J0 | 0:25 |
| 3 | Architecture: vertical slices | J1 | 0:50 |
| 4 | Setup from scratch | J2 | 2:35 |
| 5 | Live API in Scalar | J3 | 3:45 |
| 6 | AI-assisted development | J4 | 6:00 |
| 7 | Call to action | J5 | 7:20 |

## Production settings

- App type and driver: ASP.NET Core Web API; Scalar UI driven by Playwright (real running app, localhost). Code and terminal scenes use a Playwright-recorded code viewer.
- Resolution / fps: 1920x1080 / 30
- Vertical cut: yes, 1080x1920, under 60 s, burned-in captions
- Voice-over: local pipeline via `edge-tts` (Microsoft Edge neural voice `en-US-AndrewNeural`, stock voice, no cloning). No API key. NOTE: edge-tts uses Microsoft's online read-aloud service, which is not a documented licensed product; see `../marketing/legal-check.md`.
- AI voice disclosure: "Narration: AI-generated voice" on the title card and end card; add it to the video description.
- Music: none
- Branding: dark slate (#0f172a) with blue accent (#0d6efd), Segoe UI
- Captions: .srt + .vtt; burned in for the vertical cut

## Safety

- Local only (`http://localhost:5156`), LocalDB database `FastEndpointsDemo` created fresh and dropped/recreated by `automation/reset-demo.ps1`. The owner's own `FastEndpointsAPI` database is never touched.
- Demo account `demo.admin@example.com` with a demo password; JWT blurred on screen; `ApiSettings:Secret` masked in the config scene.
- No emails, tokens, machine names or user paths on screen (paths trimmed from captured output).

## Out of scope

- Customer role / 403, product update and image upload endpoints, CORS policy testing from a browser origin, API v2 (the v2 document exists but has no endpoints; shown as such).
- Automated tests (none exist in the repo; stated in the video).
