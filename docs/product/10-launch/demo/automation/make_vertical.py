"""Make the vertical 1080x1920 cut (401 -> 201, validation, JSON/XML/406) with burned-in captions.

Run from docs/product/10-launch/demo after assemble.py and make_captions.py.
"""

import json
import re
import subprocess
from pathlib import Path

base = Path(__file__).resolve().parent.parent
tl = {
    s["id"]: s["start"]
    for s in json.loads((base / "exports/timeline.json").read_text())["scenes"]
}
t0 = tl["s16"] - 0.5
t1 = tl["s19"] - 0.4
name = "vertical-slice-api-demo"

blocks = (
    (base / f"captions/{name}.srt").read_text(encoding="utf-8").strip().split("\n\n")
)


def sec(h, m, s, ms):
    return int(h) * 3600 + int(m) * 60 + int(s) + int(ms) / 1000


def ts(x):
    ms = int(round(x * 1000))
    h, ms = divmod(ms, 3600000)
    m, ms = divmod(ms, 60000)
    s, ms = divmod(ms, 1000)
    return f"{h:02}:{m:02}:{s:02},{ms:03}"


out = []
for b in blocks:
    lines = b.split("\n")
    g = re.match(
        r"(\d+):(\d+):(\d+),(\d+) --> (\d+):(\d+):(\d+),(\d+)", lines[1]
    ).groups()
    a, e = sec(*g[:4]), sec(*g[4:])
    if a >= t0 and e <= t1 + 0.2:
        out.append(
            f"{len(out) + 1}\n{ts(a - t0)} --> {ts(e - t0)}\n" + "\n".join(lines[2:])
        )
(base / f"captions/{name}-vertical.srt").write_text(
    "\n\n".join(out) + "\n", encoding="utf-8"
)

ex = base / "exports"
subprocess.run(
    [
        "ffmpeg",
        "-y",
        "-v",
        "error",
        "-i",
        f"{name}.mp4",
        "-ss",
        f"{t0:.3f}",
        "-t",
        f"{t1 - t0:.3f}",
        "-c:v",
        "libx264",
        "-crf",
        "18",
        "-c:a",
        "aac",
        "cut.mp4",
    ],
    cwd=ex,
    check=True,
)
(ex / "v.srt").write_text(
    (base / f"captions/{name}-vertical.srt").read_text(encoding="utf-8"),
    encoding="utf-8",
)
subprocess.run(
    [
        "ffmpeg",
        "-y",
        "-v",
        "error",
        "-i",
        "cut.mp4",
        "-filter_complex",
        "color=c=0x0f172a:s=1080x1920:r=30[bg];[0:v]scale=1080:-2[fg];"
        "[bg][fg]overlay=(W-w)/2:(H-h)/2-120:shortest=1,"
        "subtitles=v.srt:force_style='FontName=Segoe UI,FontSize=13,Bold=1,Outline=2,MarginV=60'[v]",
        "-map",
        "[v]",
        "-map",
        "0:a",
        "-c:v",
        "libx264",
        "-crf",
        "20",
        "-pix_fmt",
        "yuv420p",
        "-c:a",
        "aac",
        "-b:a",
        "160k",
        "-movflags",
        "+faststart",
        f"{name}-vertical.mp4",
    ],
    cwd=ex,
    check=True,
)
(ex / "v.srt").unlink()
(ex / "cut.mp4").unlink()
print(f"vertical cut {t1 - t0:.1f}s from {t0:.1f}s")
