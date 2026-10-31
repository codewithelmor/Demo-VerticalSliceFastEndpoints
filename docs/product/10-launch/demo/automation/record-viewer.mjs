// Records the code-viewer / terminal-replay / end-card scenes (everything that is not the live Scalar journey).
// All code shown is read from the real files in this repo at record time. Terminal text comes from real command
// output captured in work/captures (paths trimmed). Secret values are masked.
// Usage: node record-viewer.mjs A|B [--record]
import { chromium } from 'playwright';
import { readFileSync, existsSync, mkdirSync, renameSync, readdirSync, statSync, writeFileSync } from 'node:fs';
import { execSync } from 'node:child_process';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { demo } from './pw-demo-helpers.mjs';

const HERE = fileURLToPath(new URL('.', import.meta.url));
const DEMO = join(HERE, '..');
const REPO = join(DEMO, '..', '..', '..', '..');
const SEG = (process.argv[2] ?? 'A').toUpperCase();
const RECORD = process.argv.includes('--record');
const durations = JSON.parse(readFileSync(join(DEMO, 'audio', 'durations.json'), 'utf8'));
const scenesMeta = JSON.parse(readFileSync(join(DEMO, 'scenes.json'), 'utf8')).scenes;
const meta = Object.fromEntries(scenesMeta.map(s => [s.id, s]));
const need = id => (durations[id] ?? 4) + 1.8 + 0.4;
const read = rel => readFileSync(join(REPO, rel), 'utf8').replace(/^﻿/, '').replace(/\r\n/g, '\n');
const cap = f => readFileSync(join(DEMO, 'work', 'captures', f), 'utf8').replace(/\r\n/g, '\n').trimEnd();
const mask = t => t.replace(/("Secret"\s*:\s*")[^"]*(")/, '$1•••••••••• masked for this video ••••••••••$2');

function treeLines(paths, strip) {
  const root = {};
  for (const p of paths) { let n = root; for (const part of p.slice(strip.length).split('/')) n = n[part] ??= {}; }
  const out = [];
  const walk = (n, prefix) => {
    const keys = Object.keys(n).sort((a, b) => (Object.keys(n[b]).length > 0) - (Object.keys(n[a]).length > 0) || a.localeCompare(b));
    keys.forEach((k, i) => {
      const last = i === keys.length - 1;
      out.push(prefix + (last ? '└── ' : '├── ') + k + (Object.keys(n[k]).length ? '/' : ''));
      walk(n[k], prefix + (last ? '    ' : '│   '));
    });
  };
  walk(root, '');
  return out;
}

const html = `<!doctype html><html><head><meta charset="utf-8"><style>
*{margin:0;box-sizing:border-box}
body{width:1920px;height:1080px;overflow:hidden;background:#0d1117;color:#e6edf3;font-family:"Segoe UI",system-ui,sans-serif}
#top{height:72px;background:#161b22;border-bottom:1px solid #30363d;display:flex;align-items:center;padding:0 36px;gap:20px}
#tab{font-family:"Cascadia Mono",Consolas,monospace;font-size:26px;color:#e6edf3}
#badge{margin-left:auto;font-size:22px;color:#8b949e}
#note{font-size:22px;color:#f0b429;margin-left:24px}
#main{position:absolute;top:72px;left:0;right:0;bottom:84px;overflow:hidden}
#scroller{position:absolute;inset:0;overflow:hidden}
#inner{position:absolute;left:0;right:0;top:0;transition:transform .9s ease}
.ln{display:flex;font-family:"Cascadia Mono",Consolas,monospace;font-size:25px;line-height:37px;height:37px;white-space:pre;transition:opacity .5s,background .4s}
.ln .n{width:90px;text-align:right;padding-right:24px;color:#6e7681;flex:none}
.ln .c{flex:1;padding-right:30px;border-left:5px solid transparent;padding-left:14px}
.dim .ln{opacity:.38}
.dim .ln.hl{opacity:1;background:#1f6feb26}
.dim .ln.hl .c{border-left-color:#58a6ff}
.k{color:#ff7b72}.s{color:#a5d6ff}.cm{color:#8b949e}.t{color:#7ee787}.nu{color:#79c0ff}.tag{color:#7ee787}.at{color:#79c0ff}.h{color:#79c0ff;font-weight:700}
#term{display:none;position:absolute;inset:0;padding:30px 50px;font-family:"Cascadia Mono",Consolas,monospace;font-size:27px;line-height:40px;white-space:pre-wrap;word-break:break-all;background:#0a0e14;overflow:hidden}
#term .cmd{color:#7ee787}.out{color:#d0d7de}.warn{color:#f0b429}.ok{color:#56d364}
#cursor{display:inline-block;width:14px;height:28px;background:#d0d7de;vertical-align:-5px;animation:b 1s steps(2) infinite}@keyframes b{50%{opacity:0}}
#bot{position:absolute;left:0;right:0;bottom:0;height:84px;background:#161b22;border-top:1px solid #30363d;display:flex;align-items:center;padding:0 36px;gap:28px}
#chap{font-size:26px;color:#8b949e}#chap b{color:#58a6ff;font-weight:600}
#on{margin-left:auto;font-size:34px;font-weight:600;color:#fff;background:#1f6feb;padding:6px 22px;border-radius:10px}
#card{display:none;position:absolute;inset:0;background:#0f172a;z-index:5}
#card .t1{position:absolute;left:140px;top:250px;font-size:120px;font-weight:700;letter-spacing:-.02em;width:1640px;line-height:1.05}
#card .bar{position:absolute;left:140px;top:200px;width:90px;height:10px;background:#0d6efd;border-radius:5px}
#card .t2{position:absolute;left:140px;top:560px;font-size:46px;color:#cbd5e1;line-height:1.5;width:1640px}
#card .t3{position:absolute;left:140px;bottom:90px;font-size:30px;color:#94a3b8;line-height:1.5}
</style></head><body>
<div id="top"><div id="tab"></div><div id="note"></div><div id="badge">repo commit 9a638cc</div></div>
<div id="main"><div id="scroller"><div id="inner"></div></div><div id="term"></div></div>
<div id="bot"><div id="chap"></div><div id="on"></div></div>
<div id="card"><div class="bar"></div><div class="t1"></div><div class="t2"></div><div class="t3"></div></div>
<script>${readFileSync(join(HERE, 'viewer-page.js'), 'utf8')}</script></body></html>`;

// ---------- scene definitions ----------
const CH = { 'Introduction': '1 · Introduction', 'Technology stack': '1 · Technology stack', 'Architecture': '2 · Architecture',
  'Setup from scratch': '3 · Setup from scratch', 'Live API in Scalar': '4 · Live API', 'AI-assisted development': '5 · AI-assisted development', 'Call to action': '6 · Let’s talk' };
const chapFor = id => { const c = meta[id].chapter; return `<b>${CH[c]}</b>`; };

const featureTree = execSync('git ls-files VerticalSliceFastEndpoints/Features', { cwd: REPO, encoding: 'utf8' }).trim().split('\n');
const claudeTree = (() => {
  const base = join(REPO, '.claude'); const out = [];
  const walk = (dir, prefix, depth) => {
    const names = readdirSync(dir).sort();
    names.forEach((n, i) => {
      const full = join(dir, n); const isDir = statSync(full).isDirectory(); const last = i === names.length - 1;
      out.push(prefix + (last ? '└── ' : '├── ') + n + (isDir ? '/' : ''));
      if (isDir && depth < 1) walk(full, prefix + (last ? '    ' : '│   '), depth + 1);
    });
  };
  walk(base, '', 0); return out;
})();

// A step: { at: fraction of the voice-over, file|text, lang, tab, match, n, lines:[a,b], term: [...] }
const A = {
  s01: { tab: 'README.md', file: 'README.md', lang: 'md', steps: [{ at: 0, lines: [1, 5] }, { at: 0.55, lines: [7, 18] }] },
  s02: { tab: 'VerticalSliceFastEndpoints/VerticalSliceFastEndpoints.csproj', file: 'VerticalSliceFastEndpoints/VerticalSliceFastEndpoints.csproj', lang: 'xml', steps: [
    { at: 0.0, match: 'TargetFramework', n: 1 }, { at: 0.2, match: '"FastEndpoints"', n: 1 }, { at: 0.38, match: '"FluentValidation"', n: 1 },
    { at: 0.5, match: 'Identity.EntityFrameworkCore', n: 1 }, { at: 0.6, match: 'JwtBearer', n: 1 }, { at: 0.7, match: 'EntityFrameworkCore.SqlServer', n: 1 },
    { at: 0.8, match: '"Dapper"', n: 1 }, { at: 0.9, match: 'Scalar.AspNetCore', n: 1 }] },
  s03: { tab: 'VerticalSliceFastEndpoints/Features/', text: ['Features/', ...treeLines(featureTree, 'VerticalSliceFastEndpoints/Features/')].join('\n'), lang: 'txt', steps: [
    { at: 0.0, match: 'Auth/', n: 1 }, { at: 0.25, match: 'Product/', n: 1 }, { at: 0.4, match: 'Users/', n: 1 }, { at: 0.6, match: 'CreateProduct/', n: 5 }] },
  s04: { tab: 'Features/Product/CreateProduct/CreateProductEndpoint.cs', file: 'VerticalSliceFastEndpoints/Features/Product/CreateProduct/CreateProductEndpoint.cs', lang: 'cs', steps: [
    { at: 0.0, match: 'public override void Configure', n: 1 }, { at: 0.18, match: 'Post\\("/products"\\)', n: 1 }, { at: 0.26, match: 'Version\\(1\\)', n: 1 },
    { at: 0.34, match: 'Roles\\(ApplicationRole.Admin\\)', n: 1 }, { at: 0.44, match: 'Throttle', n: 1 }, { at: 0.55, match: 'Description\\(x => x', n: 5 },
    { at: 0.72, match: 'public override async Task HandleAsync', n: 18 }] },
  s05: { tab: 'Features/Product/CreateProduct/CreateProductValidation.cs', file: 'VerticalSliceFastEndpoints/Features/Product/CreateProduct/CreateProductValidation.cs', lang: 'cs', steps: [
    { at: 0.0, match: 'RuleFor\\(x => x.Title\\)', n: 3 }, { at: 0.55, match: 'RuleFor\\(x => x.Description\\)', n: 2 }] },
  s06: { tab: 'VerticalSliceFastEndpoints/Program.cs', file: 'VerticalSliceFastEndpoints/Program.cs', lang: 'cs', steps: [
    { at: 0.0, match: 'AddIdentity', n: 5 }, { at: 0.13, match: 'AddAuthentication', n: 26 }, { at: 0.25, match: 'AddCors', n: 12 },
    { at: 0.36, match: 'UseFastEndpoints', n: 12 }, { at: 0.52, match: 'c.Endpoints.RoutePrefix', n: 3 }, { at: 0.62, match: 'builder.Services.OpenApiDocument', n: 18 },
    { at: 0.82, match: 'c.Serializer.RequestDeserializer', n: 32 }] },
  s07: { tab: 'Data/Custom/ApplicationDbContext.Custom.cs', file: 'VerticalSliceFastEndpoints/Data/Custom/ApplicationDbContext.Custom.cs', lang: 'cs', steps: [
    { at: 0.0, match: 'private void ApplyAuditInformation', n: 3 }, { at: 0.2, match: '// 2\\) Created', n: 14 }, { at: 0.52, match: '// 1\\) Soft delete', n: 8 }] },
  s08: { tab: 'Features/Users/GetUsers/GetUsersEndpoint.cs', file: 'VerticalSliceFastEndpoints/Features/Users/GetUsers/GetUsersEndpoint.cs', lang: 'cs', steps: [
    { at: 0.0, match: 'const string sql', n: 21 }, { at: 0.6, match: 'await db.QueryAsync', n: 4 }] },
  s09: { term: true, tab: 'Captured output (replayed)', note: 'Real command output, paths trimmed', items: [
    ['cmd', 'dotnet --version'], ['out', () => cap('ver.txt')],
    ['cmd', 'type global.json'], ['out', () => read('global.json').trim()],
    ['cmd', 'dotnet build'], ['out', () => cap('build_view.txt')]] },
  s10: { tab: 'VerticalSliceFastEndpoints/appsettings.json', note: 'ApiSettings:Secret masked in this video', file: 'VerticalSliceFastEndpoints/appsettings.json', lang: 'json', mask: true, steps: [
    { at: 0.0, match: 'ConnectionStrings', n: 3 }, { at: 0.2, match: 'ApiSettings', n: 5 },
    { at: 0.62, switch: { tab: 'VerticalSliceFastEndpoints/appsettings.Development.json', note: '', file: 'VerticalSliceFastEndpoints/appsettings.Development.json', lang: 'json' }, match: 'DatabaseInitializer', n: 5 }] },
  s11: { term: true, tab: 'Captured output (replayed)', note: 'Real startup log, EF SQL lines and paths filtered out', items: [
    ['cmd', '$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\\mssqllocaldb;Database=FastEndpointsDemo;..."'],
    ['cmd', 'dotnet run --project VerticalSliceFastEndpoints'], ['out', () => cap('run.txt')]] },
  s21: { term: true, tab: 'Captured output (replayed)', note: 'Real sqlcmd result from the demo database', items: [
    ['cmd', 'sqlcmd -d FastEndpointsDemo -Q "SELECT Title, IsDeleted, CreatedDate, CreatedBy, DeletedDate, DeletedBy FROM Products ..."'],
    ['out', () => sqlTable()]] },
  s22: { tab: 'CLAUDE.md', file: 'CLAUDE.md', lang: 'md', steps: [
    { at: 0.0, match: '## Overview', n: 3 }, { at: 0.2, match: '## Commands', n: 3 }, { at: 0.38, match: '## Architecture', n: 3 },
    { at: 0.62, match: 'Don.t hard-delete', n: 1 }] },
  s23: { tab: 'CLAUDE.md', file: 'CLAUDE.md', lang: 'md', steps: [
    { at: 0.0, match: '## Code Generation Guidelines', n: 3 }, { at: 0.42, match: '/docs/api-configuration.md', n: 4 }] },
  s24: { tab: '.claude/', text: ['.claude/', ...claudeTree].join('\n'), lang: 'txt', steps: [
    { at: 0.0, match: 'agents/', n: 2 }, { at: 0.2, match: 'commands/', n: 3 }, { at: 0.55, match: 'skills/', n: 8 }] },
  s25: { tab: '.claude/settings.json', file: '.claude/settings.json', lang: 'json', steps: [{ at: 0.0, match: '"Stop"', n: 10 }] },
  s26: { tab: 'CLAUDE.md', file: 'CLAUDE.md', lang: 'md', steps: [
    { at: 0.0, match: 'There are no tests in the repo', n: 1 },
    { at: 0.5, switchTerm: { tab: 'Captured output (replayed)', note: 'Real build output, paths trimmed', items: [['cmd', 'dotnet build'], ['out', () => cap('build_view.txt')]] } }] },
  s27: { card: true },
};

function sqlTable() {
  const rows = cap('sql.txt').split('\n').filter(l => l.includes('|')).map(l => l.split('|'));
  const w = rows[0].map((_, i) => Math.max(...rows.map(r => r[i].length)));
  return rows.map((r, k) => r.map((c, i) => c.padEnd(w[i])).join('  ') + (k === 0 ? '\n' + w.map(x => '-'.repeat(x)).join('  ') : '')).join('\n') + '\n\n(1 row)';
}

// build_view.txt: the captured build lines, with machine paths removed
{
  const raw = readFileSync(join(DEMO, 'work', 'captures', 'build_raw.txt'), 'utf8').replace(/\r\n/g, '\n').split('\n');
  const keep = [];
  for (let l of raw) {
    if (!/(NU190|Build succeeded|Warning\(s\)|Error\(s\)|Elapsed)/.test(l)) continue;
    const i = l.indexOf('VerticalSliceFastEndpoints.csproj');
    if (i > -1) { l = l.slice(i); const j = l.lastIndexOf(' ['); if (j > -1) l = l.slice(0, j); }
    l = l.trim(); if (!keep.includes(l) && !l.includes('Cryptography.Pkcs\' 9.0.11 has a known moderate')) keep.push(l);
  }
  writeFileSync(join(DEMO, 'work', 'captures', 'build_view.txt'), keep.join('\n') + '\n');
}

const order = SEG === 'A' ? ['s01', 's02', 's03', 's04', 's05', 's06', 's07', 's08', 's09', 's10', 's11'] : ['s21', 's22', 's23', 's24', 's25', 's26', 's27'];
const OUT = join(DEMO, 'recordings', `viewer-${SEG}`);
mkdirSync(OUT, { recursive: true });

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: { width: 1920, height: 1080 }, ...(RECORD ? { recordVideo: { dir: OUT, size: { width: 1920, height: 1080 } } } : {}) });
const page = await context.newPage();
const d = demo(page, { shotsDir: join(DEMO, 'work', 'failures') });
await page.setContent(html, { waitUntil: 'load' });

const findRange = (text, step) => {
  const ls = text.split('\n'); const re = new RegExp(step.match);
  const i = ls.findIndex(l => re.test(l));
  if (i < 0) throw new Error(`no line matches ${step.match}`);
  return [i + 1, Math.min(ls.length, i + (step.n ?? 1))];
};
const wait = ms => page.waitForTimeout(Math.max(0, ms));

async function showFile(s) {
  let text = s.text ?? read(s.file);
  if (s.mask) text = mask(text);
  await page.evaluate(([t, l]) => setCode(t, l), [text, s.lang]);
  return text;
}
async function runTerm(def, seconds) {
  await page.evaluate(() => setTerm());
  const t0 = Date.now();
  const items = def.items.map(([k, v]) => [k, typeof v === 'function' ? v() : v]);
  const outLines = items.filter(i => i[0] === 'out').reduce((n, i) => n + i[1].split('\n').length, 0);
  const budget = seconds * 1000 * 0.6; const per = Math.min(260, budget / Math.max(1, outLines));
  for (const [k, v] of items) {
    if (k === 'cmd') { await page.evaluate(t => typeCmd(t), v); await wait(350); }
    else for (const line of v.split('\n')) {
      const cls = /warning|NU190/i.test(line) && !/0 Warn/.test(line) ? 'warn' : /Build succeeded|Seeded|Now listening|0 Error/.test(line) ? 'ok' : 'out';
      await page.evaluate(([c, t]) => addTerm(c, t), [cls, line]); await wait(per);
    }
  }
  return t0;
}

try {
  await page.waitForTimeout(500);
  for (const id of order) {
    const def = A[id]; const secs = need(id); const aud = durations[id];
    const m = meta[id];
    await page.evaluate(([c, o]) => { setChrome('', c, o, ''); }, [chapFor(id), m.onscreen]);
    d.scene(id, secs);
    const t0 = Date.now();
    if (def.card) {
      await page.evaluate(() => { document.getElementById('top').style.display = 'none'; document.getElementById('bot').style.display = 'none'; document.getElementById('main').style.display = 'none';
        showCard('Hiring engineers who use AI with discipline? Let’s talk.',
          'Elmor Cabalfin<br>Vertical Slice API · .NET 10 · FastEndpoints<br>github.com/codewithelmor/VerticalSliceFastEndpoints',
          'Narration: AI-generated voice (Microsoft Edge neural voice, en-US-AndrewNeural)<br>Shows repo commit 9a638cc · local demo data only'); });
      await d.endScene(); continue;
    }
    await page.evaluate(([t, n]) => { setChrome(t, document.getElementById('chap').innerHTML, document.getElementById('on').textContent, n); }, [def.tab, def.note ?? '']);
    if (def.term) { await runTerm(def, aud); await d.endScene(); continue; }
    let text = await showFile(def);
    for (const st of def.steps) {
      await wait(t0 + st.at * aud * 1000 - Date.now());
      if (st.switch) { Object.assign(def, st.switch); await page.evaluate(([t, n]) => setChrome(t, document.getElementById('chap').innerHTML, document.getElementById('on').textContent, n), [st.switch.tab, st.switch.note]); text = await showFile(st.switch); }
      if (st.switchTerm) { await page.evaluate(([t, n]) => setChrome(t, document.getElementById('chap').innerHTML, document.getElementById('on').textContent, n), [st.switchTerm.tab, st.switchTerm.note]); await runTerm(st.switchTerm, aud * 0.5); continue; }
      const [a, b] = st.lines ?? findRange(text, st);
      await page.evaluate(([x, y]) => focusLines(x, y), [a, b]);
      console.log(`PASS ${id} highlight ${a}-${b}`);
    }
    await d.endScene();
  }
  console.log(`JOURNEY PASS viewer-${SEG}`);
} catch (e) {
  console.log(`JOURNEY FAIL viewer-${SEG} - ${e.message.split('\n')[0]}`); process.exitCode = 1;
} finally {
  d.saveMarkers(join(OUT, 'markers.json'));
  const video = page.video(); await context.close();
  if (RECORD && video && process.exitCode !== 1) { renameSync(await video.path(), join(OUT, 'raw.webm')); console.log('Saved raw.webm'); }
  await browser.close();
}
