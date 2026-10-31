"""Write script.md from scenes.json and audio/durations.json."""

import json
from pathlib import Path

base = Path(__file__).resolve().parent.parent
scenes = json.loads((base / "scenes.json").read_text(encoding="utf-8"))["scenes"]
dur_file = base / "audio" / "durations.json"
dur = json.loads(dur_file.read_text()) if dur_file.exists() else {}

lines = [
    "# Demo script: VerticalSliceFastEndpoints (repo commit 9a638cc)",
    "",
    "Voice: en-US-AndrewNeural (Microsoft neural stock voice, AI-generated, not a clone). About 148 words per minute.",
    "The `Voice-over` column is the spoken text. The captions use the on-screen spelling (see `scenes.json`, field `caption`).",
    "Scenes marked *Scalar* are the real app running on localhost against a throwaway LocalDB database.",
    "Scenes marked *Code viewer* show real repo files. Scenes marked *Terminal replay* show real command output captured earlier, replayed on screen and labelled as such.",
    "",
]
chapter = None
total = 0.0
for s in scenes:
    if s["chapter"] != chapter:
        chapter = s["chapter"]
        lines += [f"## {chapter}", ""]
    d = dur.get(s["id"], 0)
    total += d
    words = len(s["voiceover"].split())
    lines += [
        f"### {s['id']} - {s['title']}  ({s['journey']}, {d:.1f} s voice, {words} words)",
        "",
        f"- **Screen:** {s['screen']}",
        f"- **Actions:** {s['actions']}",
        f"- **Result held on screen:** {s['result']}",
        f"- **On-screen text:** {s['onscreen']}",
        f"- **Voice-over:** {s['caption']}",
        "",
    ]
lines += [
    f"Total voice-over: {total:.0f} s ({total / 60:.1f} min) before holds and title card.",
    "",
]
(base / "script.md").write_text("\n".join(lines), encoding="utf-8")
print("script.md written")
