"""Create one voice-over WAV per scene with edge-tts (Microsoft neural voice) and write audio/durations.json.

Usage: python make_voiceover.py [--voice en-US-AndrewNeural] [--rate -5%]
Needs: pip install edge-tts ; ffmpeg on PATH. No API key is used.
Stock voice only. No cloning or imitation.
"""

import argparse
import asyncio
import hashlib
import json
import subprocess
import sys
from pathlib import Path

import edge_tts

BASE = Path(__file__).resolve().parent.parent


def ffprobe_duration(path: Path) -> float:
    out = subprocess.run(
        [
            "ffprobe",
            "-v",
            "error",
            "-show_entries",
            "format=duration",
            "-of",
            "default=nw=1:nk=1",
            str(path),
        ],
        capture_output=True,
        text=True,
        check=True,
    )
    return float(out.stdout.strip())


async def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--voice", default="en-US-AndrewNeural")
    ap.add_argument("--rate", default="-5%")
    ap.add_argument("--force", action="store_true", help="regenerate scenes that already have a WAV")
    args = ap.parse_args()

    scenes = json.loads((BASE / "scenes.json").read_text(encoding="utf-8"))["scenes"]
    audio = BASE / "audio"
    audio.mkdir(exist_ok=True)
    durations: dict[str, float] = {}
    hash_file = audio / "_hashes.json"
    hashes: dict[str, str] = json.loads(hash_file.read_text()) if hash_file.exists() else {}
    for s in scenes:
        mp3 = audio / f"{s['id']}.mp3"
        wav = audio / f"{s['id']}.wav"
        key = hashlib.sha1(f"{args.voice}|{args.rate}|{s['voiceover']}".encode()).hexdigest()
        if wav.exists() and not args.force and hashes.get(s["id"]) == key:
            durations[s["id"]] = round(await asyncio.to_thread(ffprobe_duration, wav), 3)
            print(f"{s['id']}: kept existing {durations[s['id']]:.2f}s")
            continue
        for attempt in range(5):  # the online service sometimes returns no audio; retry
            try:
                await edge_tts.Communicate(s["voiceover"], args.voice, rate=args.rate).save(str(mp3))
                break
            except edge_tts.exceptions.NoAudioReceived:
                await asyncio.sleep(2 + attempt * 2)
        else:
            raise SystemExit(f"No audio for {s['id']} after 5 tries")
        # 48 kHz mono 16-bit; trim silence at start and end; keep 0.15 s of air
        af = (
            "silenceremove=start_periods=1:start_threshold=-45dB:start_silence=0.12,"
            "areverse,silenceremove=start_periods=1:start_threshold=-45dB:start_silence=0.12,areverse"
        )
        subprocess.run(
            [
                "ffmpeg",
                "-y",
                "-v",
                "error",
                "-i",
                str(mp3),
                "-af",
                af,
                "-ar",
                "48000",
                "-ac",
                "1",
                "-sample_fmt",
                "s16",
                str(wav),
            ],
            check=True,
        )
        mp3.unlink()
        hashes[s["id"]] = key
        hash_file.write_text(json.dumps(hashes, indent=2))
        durations[s["id"]] = round(await asyncio.to_thread(ffprobe_duration, wav), 3)
        words = len(s["voiceover"].split())
        print(
            f"{s['id']}: {durations[s['id']]:.2f}s  {words} words  {words / durations[s['id']] * 60:.0f} wpm"
        )
    (audio / "durations.json").write_text(
        json.dumps(durations, indent=2), encoding="utf-8"
    )
    total = sum(durations.values())
    words = sum(len(s["voiceover"].split()) for s in scenes)
    print(f"TOTAL speech {total:.1f}s, {words} words, {words / total * 60:.0f} wpm")


if __name__ == "__main__":
    sys.exit(asyncio.run(main()))
