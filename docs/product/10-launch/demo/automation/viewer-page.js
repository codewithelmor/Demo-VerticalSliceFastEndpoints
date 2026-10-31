const esc = s => s.replace(/&/g, '&amp;').replace(/</g, '&lt;');
const KW = new Set('public private protected internal static class override async await var new return namespace using if else null true false const readonly sealed partial void foreach in this string bool int get set record global'.split(' '));
function tok(line, re, fn) {
  let out = '', last = 0, m; re.lastIndex = 0;
  while ((m = re.exec(line))) { out += esc(line.slice(last, m.index)) + fn(m); last = m.index + m[0].length; if (m[0] === '') re.lastIndex++; }
  return out + esc(line.slice(last));
}
function hl(line, lang) {
  if (lang === 'cs') return tok(line, /(\/\/.*$)|("(?:[^"\\]|\\.)*")|\b([A-Za-z_]\w*)\b/g, m => m[1] ? '<span class="cm">' + esc(m[1]) + '</span>' : m[2] ? '<span class="s">' + esc(m[2]) + '</span>' : KW.has(m[3]) ? '<span class="k">' + m[3] + '</span>' : esc(m[0]));
  if (lang === 'json') return tok(line, /("[^"]*")(\s*:)?/g, m => '<span class="' + (m[2] ? 'at' : 's') + '">' + esc(m[1]) + '</span>' + esc(m[2] || ''));
  if (lang === 'xml') return tok(line, /(<\/?)([\w.]+)|([\w:.]+)=("[^"]*")/g, m => m[2] ? esc(m[1]) + '<span class="tag">' + m[2] + '</span>' : '<span class="at">' + m[3] + '</span>=<span class="s">' + esc(m[4]) + '</span>');
  if (lang === 'md') { if (/^#{1,6} /.test(line)) return '<span class="h">' + esc(line) + '</span>'; return tok(line, /`[^`]+`/g, m => '<span class="s">' + esc(m[0]) + '</span>'); }
  return esc(line);
}
let lines = [];
const tab_ = document.getElementById('tab'), chap_ = document.getElementById('chap'), on_ = document.getElementById('on'), note_ = document.getElementById('note');
const scroller = document.getElementById('scroller'), inner = document.getElementById('inner'), term = document.getElementById('term'), card = document.getElementById('card');
window.setChrome = (tab, chap, on, note) => { tab_.textContent = tab; chap_.innerHTML = chap; on_.textContent = on || ''; on_.style.display = on ? 'block' : 'none'; note_.textContent = note || ''; };
window.showCard = (t1, t2, t3) => { card.style.display = 'block'; document.querySelector('#card .t1').textContent = t1; document.querySelector('#card .t2').innerHTML = t2; document.querySelector('#card .t3').innerHTML = t3; };
window.setCode = (text, lang) => {
  term.style.display = 'none'; scroller.style.display = 'block'; inner.style.transform = 'translateY(0)'; inner.classList.remove('dim');
  lines = text.split('\n');
  inner.innerHTML = lines.map((l, i) => '<div class="ln" id="L' + (i + 1) + '"><span class="n">' + (i + 1) + '</span><span class="c">' + hl(l, lang) + '</span></div>').join('');
};
window.focusLines = (a, b) => {
  inner.classList.add('dim');
  inner.querySelectorAll('.ln.hl').forEach(e => e.classList.remove('hl'));
  for (let i = a; i <= b; i++) { const e = document.getElementById('L' + i); if (e) e.classList.add('hl'); }
  const H = document.getElementById('main').clientHeight, total = lines.length * 37;
  const mid = ((a + b) / 2 - 0.5) * 37; let y = Math.max(0, Math.min(total - H, mid - H / 2)); if (total <= H) y = 0;
  inner.style.transform = 'translateY(' + (-y) + 'px)';
};
window.clearFocus = () => { inner.classList.remove('dim'); };
window.setTerm = () => { scroller.style.display = 'none'; term.style.display = 'block'; term.innerHTML = ''; };
window.addTerm = (cls, text) => { const d = document.createElement('div'); d.className = cls; d.textContent = text; term.appendChild(d); return d; };
window.typeCmd = async text => {
  const d = addTerm('cmd', ''); const cur = document.createElement('span'); cur.id = 'cursor'; d.appendChild(cur);
  for (const ch of text) { cur.before(document.createTextNode(ch)); await new Promise(r => setTimeout(r, 28)); }
  cur.remove();
};
