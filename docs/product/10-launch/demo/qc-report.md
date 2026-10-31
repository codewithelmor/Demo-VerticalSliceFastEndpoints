# Demo quality check

Video: `exports/vertical-slice-api-demo.mp4` | SHA-256: `1220ec5034660a8b63f8451ef27eefb487ace343bb913bc0818a6d424b4a271a` | App version: repo commit 9a638cc (the repo has no version string; cards say "repo commit 9a638cc") | Checked: 2026-10-31
Vertical cut: `exports/vertical-slice-api-demo-vertical.mp4` | SHA-256: `0659d1958502ea1efd687739b905b0e2973c52f3e8a38e25dee8477385beb496` (captions burned in; 51.7 s; 1080x1920)

Automated checks: `check_video.py` on the main video: 0 High failures, 1 warning (frozen frames, explained in row 14). Raw output in `work/qc-auto.md` (gitignored).

| # | Check | Severity | Result | Notes |
|---|---|---|---|---|
| 1 | Every planned journey shown start to finish | High | Pass | J0-J5 all present; J3 ran 56 automated steps, each with an HTTP status or visible-text check |
| 2 | Each journey ends with its result visible for 1.5 s or more | High | Pass | Every scene holds 1.8 s after the narration |
| 3 | No failed, skipped, or hidden steps; no error dialogs | High | Pass | Run logs `JOURNEY PASS` for J3, viewer-A, viewer-B. The only error responses on screen are the planned 400, 401, 406 |
| 4 | No private data (names, emails, tokens, keys, user paths, machine names) | High | Pass | Reviewed contact sheets and full-size frames at login and token entry: JWT blurred, secret masked, paths trimmed. Visible by design: demo account `demo.admin@example.com` (example.com), owner name and repo URL on the end card |
| 5 | No notifications, other apps, bookmarks | High | Pass | Fresh headless browser, page-only recording |
| 6 | Captions present (.srt, .vtt), timed, spelled correctly | High | Pass | 119 cues, 0 overlaps; spelling taken from on-screen text (`caption` field) |
| 7 | Version on cards matches build | High | Pass | "repo commit 9a638cc" on title card, viewer header, end card; `git log` HEAD is 9a638cc |
| 8 | H.264 High, yuv420p, AAC 48 kHz, faststart | High | Pass | |
| 9 | 1920x1080 / 30 fps | Medium | Pass | |
| 10 | Length within plan (+-15 %) | Medium | Pass | 8:26 (506 s); brief asked for 6-9 min |
| 11 | Loudness -18 to -14 LUFS, true peak <= -1 dB | Medium | Pass | -16.2 LUFS, -1.4 dBTP |
| 12 | Voice matches screen (sync) | Medium | Pass (with note) | Scalar scenes start narration after setup via per-scene `voiceDelay`; spot checks at s13, s16, s18 matched. The end card holds about 21 s (longer than needed) |
| 13 | Pacing: readable typing, clear cursor, holds | Medium | Pass | |
| 14 | No long black or frozen parts (> 6 s) | Medium | Warn | Frozen frames are code or terminal views that stay still while narration plays. No still part without narration |
| 15 | Branding consistent | Low | Pass | |
| 16 | Title and end card text true and spelled correctly; AI voice disclosed | Medium | Pass | "Narration: AI-generated voice" on title and end cards |
| 17 | Music licensed | High | Pass | No music |

**Result:** Pass (automated and visual). Open items are in the owner's manual list, not video defects:
- Voice service licensing (`marketing/legal-check.md`) before public posting.
- The code, config, and terminal scenes are replays of real repo files and real captured command output (labelled "Captured output (replayed)" on screen). They are not live terminals.
- Vertical cut was checked with the automated tool for codec, size, loudness only (captions are burned in, so no .srt was passed).
