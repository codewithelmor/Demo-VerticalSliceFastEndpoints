"""Assemble a demo video with FFmpeg.

Usage: python assemble.py edit.json

edit.json (paths relative to the edit.json folder):
{
  "name": "demo-v1.0.0",
  "size": "1920x1080", "fps": 30,
  "segments": [
    { "video": "recordings/j1/raw.webm", "markers": "recordings/j1/markers.json", "trimStart": 0.3 }
  ],
  "audioDir": "audio",            # <scene-id>.wav per scene (optional)
  "voiceLead": 0.3,               # seconds after scene start before the voice begins
  "titleCard": "thumbnail/title.png", "titleSeconds": 3,
  "endCard": "thumbnail/end.png",     "endSeconds": 4,
  "chapters": [ { "title": "Add an item", "scene": "s01" } ],
  "exportsDir": "exports"
}

Writes <exportsDir>/<name>.mp4, timeline.json (scene starts in the final video),
and chapters.txt (for Loom or YouTube descriptions). Standard library only.
"""
import json, os, subprocess, sys

def run(args):
    r = subprocess.run(args, capture_output=True, text=True)
    if r.returncode != 0:
        sys.exit(f"FFmpeg failed:\n{' '.join(args)}\n{r.stderr[-3000:]}")
    return r

def duration(path):
    r = run(['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'default=nw=1:nk=1', path])
    return float(r.stdout.strip())

def card(png, seconds, out, w, h, fps):
    run(['ffmpeg', '-y', '-loop', '1', '-t', str(seconds), '-i', png,
         '-f', 'lavfi', '-t', str(seconds), '-i', 'anullsrc=r=48000:cl=mono',
         '-vf', f'scale={w}:{h}:force_original_aspect_ratio=decrease,pad={w}:{h}:(ow-iw)/2:(oh-ih)/2,fps={fps},format=yuv420p',
         '-c:v', 'libx264', '-crf', '18', '-c:a', 'aac', '-b:a', '160k', '-shortest', out])

def fmt_time(s):
    s = int(round(s)); return f'{s // 60}:{s % 60:02}'

def main():
    cfg_path = os.path.abspath(sys.argv[1])
    base = os.path.dirname(cfg_path)
    cfg = json.load(open(cfg_path, encoding='utf-8-sig'))
    p = lambda rel: os.path.join(base, rel)
    w, h = map(int, cfg.get('size', '1920x1080').split('x'))
    fps = cfg.get('fps', 30)
    out_dir = p(cfg.get('exportsDir', 'exports')); tmp = os.path.join(out_dir, 'tmp')
    os.makedirs(tmp, exist_ok=True)
    audio_dir = p(cfg['audioDir']) if cfg.get('audioDir') else None
    lead = cfg.get('voiceLead', 0.3)
    title_s = cfg.get('titleSeconds', 3) if cfg.get('titleCard') else 0

    parts, timeline, offset = [], [], title_s
    if title_s:
        card(p(cfg['titleCard']), title_s, os.path.join(tmp, 'title.mp4'), w, h, fps)
        parts.append(os.path.join(tmp, 'title.mp4'))

    for i, seg in enumerate(cfg['segments']):
        trim = seg.get('trimStart', 0)
        vid = os.path.join(tmp, f'seg{i}-v.mp4')
        run(['ffmpeg', '-y', '-ss', str(trim), '-i', p(seg['video']), '-an',
             '-vf', f'fps={fps},scale={w}:{h}:force_original_aspect_ratio=decrease,pad={w}:{h}:(ow-iw)/2:(oh-ih)/2,format=yuv420p',
             '-c:v', 'libx264', '-crf', '18', '-preset', 'medium', vid])
        d = duration(vid)
        markers = json.load(open(p(seg['markers']), encoding='utf-8-sig'))['scenes'] if seg.get('markers') else []
        inputs = ['-i', vid, '-f', 'lavfi', '-t', f'{d:.3f}', '-i', 'anullsrc=r=48000:cl=mono']
        filters, labels = [], ['[1:a]']
        n = 2
        for m in markers:
            start = max(0.0, m['start'] - trim)
            vd = m.get('voiceDelay', 0)
            timeline.append({'id': m['id'], 'start': round(offset + start + lead + vd, 3)})
            wav = os.path.join(audio_dir, f"{m['id']}.wav") if audio_dir else None
            if wav and os.path.exists(wav):
                ms = int((start + lead + vd) * 1000)
                inputs += ['-i', wav]
                filters.append(f'[{n}:a]aresample=48000,aformat=channel_layouts=mono,adelay={ms}:all=1[a{n}]')
                labels.append(f'[a{n}]'); n += 1
            elif audio_dir:
                print(f"WARN no audio for scene {m['id']}")
        filters.append(f"{''.join(labels)}amix=inputs={len(labels)}:normalize=0:duration=first[aout]")
        seg_out = os.path.join(tmp, f'seg{i}.mp4')
        run(['ffmpeg', '-y', *inputs, '-filter_complex', ';'.join(filters), '-map', '0:v', '-map', '[aout]',
             '-c:v', 'copy', '-c:a', 'aac', '-b:a', '160k', '-t', f'{d:.3f}', seg_out])
        for m in markers:
            if m.get('end') and m['end'] - trim > d + 0.5:
                print(f"WARN scene {m['id']} ends after the video ({m['end'] - trim:.1f}s > {d:.1f}s)")
        parts.append(seg_out); offset += d

    if cfg.get('endCard'):
        end_s = cfg.get('endSeconds', 4)
        card(p(cfg['endCard']), end_s, os.path.join(tmp, 'end.mp4'), w, h, fps)
        parts.append(os.path.join(tmp, 'end.mp4')); offset += end_s
    total = offset

    # Chapters
    starts = {t['id']: t['start'] - lead for t in timeline}  # local copy: supports per-scene voiceDelay
    chapters = [{'title': 'Introduction', 'start': 0.0}] if title_s else []
    for c in cfg.get('chapters', []):
        if c['scene'] in starts:
            chapters.append({'title': c['title'], 'start': max(0.0, starts[c['scene']])})
    chapters.sort(key=lambda c: c['start'])
    meta = os.path.join(tmp, 'meta.txt')
    with open(meta, 'w', encoding='utf-8') as f:
        f.write(';FFMETADATA1\n')
        for k, c in enumerate(chapters):
            end = chapters[k + 1]['start'] if k + 1 < len(chapters) else total
            f.write(f"[CHAPTER]\nTIMEBASE=1/1000\nSTART={int(c['start'] * 1000)}\nEND={int(end * 1000)}\ntitle={c['title']}\n")

    # Join, normalize loudness, add chapters
    name = cfg.get('name', 'demo')
    final = os.path.join(out_dir, f'{name}.mp4')
    ins = []
    for part in parts: ins += ['-i', part]
    ins += ['-i', meta]
    concat = ''.join(f'[{k}:v][{k}:a]' for k in range(len(parts))) + f'concat=n={len(parts)}:v=1:a=1[v][a]'
    run(['ffmpeg', '-y', *ins, '-filter_complex', f'{concat};[a]loudnorm=I=-16:TP=-1.5:LRA=11[an]',
         '-map', '[v]', '-map', '[an]', '-map_chapters', str(len(parts)),
         '-c:v', 'libx264', '-profile:v', 'high', '-crf', '20', '-preset', 'medium', '-pix_fmt', 'yuv420p',
         '-r', str(fps), '-c:a', 'aac', '-b:a', '160k', '-ar', '48000', '-movflags', '+faststart', final])

    json.dump({'video': os.path.basename(final), 'duration': round(duration(final), 3), 'scenes': timeline},
              open(os.path.join(out_dir, 'timeline.json'), 'w'), indent=2)
    with open(os.path.join(out_dir, 'chapters.txt'), 'w', encoding='utf-8') as f:
        for c in chapters: f.write(f"{fmt_time(c['start'])} {c['title']}\n")
    print(f'Wrote {final} ({duration(final):.1f}s), timeline.json, chapters.txt')

if __name__ == '__main__':
    main()
