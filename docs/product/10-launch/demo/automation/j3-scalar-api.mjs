// Journey J3: live API walkthrough in Scalar against the running demo instance (localhost only).
// Usage: node j3-scalar-api.mjs [--record] [--audio ../audio/durations.json]
// Every step is checked (HTTP status and visible text). A failed check stops the run.
import { chromium } from 'playwright';
import { readFileSync, existsSync, mkdirSync, renameSync, writeFileSync } from 'node:fs';
import { demo } from './pw-demo-helpers.mjs';

const BASE = process.env.DEMO_BASE ?? 'http://localhost:5156';
const RECORD = process.argv.includes('--record');
const OUT = new URL('../recordings/j3-scalar/', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1');
const durations = existsSync(new URL('../audio/durations.json', import.meta.url))
  ? JSON.parse(readFileSync(new URL('../audio/durations.json', import.meta.url), 'utf8')) : {};
const HOLD = 1.6; // seconds of result hold after the voice-over
const need = id => (durations[id] ?? 4) + HOLD + 0.4;

// Demo-only credentials for a throwaway local database. Not real accounts.
const ADMIN = { email: 'demo.admin@example.com', password: 'Demo-Passw0rd!2026', role: 'Admin' };

// Blur anything that looks like a JWT so no token is readable in the recording.
const BLUR_TOKENS = () => {
  const re = /eyJ[\w-]{10,}\.[\w-]{10,}\./;
  const sweep = () => {
    document.querySelectorAll('.cm-line, span, pre, code, div, input, textarea').forEach(el => {
      if (el.dataset && el.dataset.demoBlur) return;
      const isInput = el.tagName === 'INPUT' || el.tagName === 'TEXTAREA';
      const txt = isInput ? el.value : (el.children.length === 0 ? el.textContent : '');
      if (txt && re.test(txt)) { el.style.filter = 'blur(7px)'; el.dataset.demoBlur = '1'; }
    });
  };
  setInterval(sweep, 120);
};

mkdirSync(OUT, { recursive: true });
const browser = await chromium.launch();
const context = await browser.newContext({
  viewport: { width: 1920, height: 1080 },
  ...(RECORD ? { recordVideo: { dir: OUT, size: { width: 1920, height: 1080 } } } : {}),
});
const page = await context.newPage();
const d = demo(page, { shotsDir: new URL('../work/failures/', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1') });
await d.init();
await page.addInitScript(BLUR_TOKENS);

const log = [];
const api = (method, pathPart) => page.waitForResponse(
  r => r.request().method() === method && r.url().includes(pathPart), { timeout: 15000 });
const modal = () => page.getByRole('dialog');

async function openOp(group, pathText, method) {
  const abbr = method.toLowerCase() === 'delete' ? 'del' : method;
  const esc = pathText.replace(/[/{}]/g, m => '\\' + m);
  const link = page.locator('a').filter({ hasText: new RegExp('^\\s*' + esc + 'HTTP Method:\\s*' + abbr + '\\s*$', 'i') }).first();
  if (!(await link.isVisible().catch(() => false))) {
    await d.click(`Expand ${group} group`, page.getByRole('button', { name: new RegExp(`Open Group - ${group}`) }));
  }
  await d.click(`Select ${method.toUpperCase()} ${pathText} in the sidebar`, link);
  await page.waitForTimeout(500);
  await d.click(`Open Test Request for ${pathText}`,
    page.getByRole('button', { name: new RegExp(`Test Request.*\\(${method} ${pathText.replace(/[/{}]/g, '\\$&')}\\)`, 'i') }).first(),
    async () => { await modal().waitFor({ state: 'visible', timeout: 8000 }); });
}

async function setBody(label, json) {
  await d.run(label, async () => {
    const ed = modal().locator('.cm-content').first();
    await d.moveTo(ed);
    await ed.click();
    await page.keyboard.press('Control+A');
    await page.keyboard.insertText(json);
    await d.pause(500);
  });
}

async function send(label, method, pathPart, expectStatus) {
  let res;
  await d.run(label, async () => {
    const wait = api(method, pathPart);
    wait.catch(() => {});
    const sendBtn = modal().locator('button.scalar-button-solid:visible', { hasText: 'Send' }).first();
    await d.moveTo(sendBtn);
    await d.pause(200);
    await sendBtn.click();
    res = await wait;
    await d.pause(900);
  }, async () => {
    if (res.status() !== expectStatus) throw new Error(`expected HTTP ${expectStatus}, got ${res.status()}`);
  });
  log.push(`${method} ${pathPart} -> ${res.status()}`);
  return res;
}

async function closeModal() {
  await d.run('Close the request panel', async () => {
    await page.keyboard.press('Escape');
    await modal().waitFor({ state: 'hidden', timeout: 5000 });
    await d.pause(400);
  });
}

try {
  // ---------- s12: open Scalar, versions ----------
  await page.goto(`${BASE}/scalar`);
  await page.getByRole('button', { name: 'Product API v1' }).first().waitFor({ state: 'visible', timeout: 15000 });
  await d.pause(800);
  d.scene('s12', need('s12'));
  await d.pause(2500);
  await d.run('Page shows v1 operations', async () => {
    await page.getByText('/api/v1/Users').first().waitFor({ state: 'visible' });
  });
  await d.click('Open the document picker', page.getByRole('button', { name: 'Product API v1' }).first(),
    async () => { await page.getByText('Product API v2').first().waitFor({ state: 'visible' }); });
  await d.click('Select Product API v2', page.getByText('Product API v2').first(), async () => {
    await page.getByRole('heading', { name: 'Product API v2' }).first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.run('v2 document has no operations', async () => {
    const n = await page.getByText('/api/v2/').count();
    if (n !== 0) throw new Error(`found ${n} v2 operations`);
  });
  await d.hold(2500);
  await d.click('Open the document picker', page.getByRole('button', { name: /Product API v2/ }).first());
  await d.click('Back to Product API v1', page.getByText('Product API v1').last(), async () => {
    await page.getByText('/api/v1/Users').first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.endScene();

  // ---------- s13: register ----------
  d.scene('s13', need('s13'));
  await openOp('Authentication', '/api/v1/auth/register', 'post');
  await setBody('Type the register body', JSON.stringify(ADMIN, null, 2));
  d.voiceNow();
  await send('Send register', 'POST', '/api/v1/auth/register', 200);
  await d.run('Response shows isSuccess true', async () => {
    await modal().getByText('"isSuccess": true').first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.endScene();

  // ---------- s14: validation errors ----------
  d.scene('s14', need('s14'));
  await setBody('Type an invalid register body', JSON.stringify({ email: 'not-an-email', password: 'short', role: 'Customer' }, null, 2));
  d.voiceNow();
  await send('Send invalid register', 'POST', '/api/v1/auth/register', 400);
  await d.run('Response lists password rule errors', async () => {
    await modal().getByText('Password must be at least 12 characters long.').first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.endScene();
  await closeModal();

  // ---------- s15: login ----------
  d.scene('s15', need('s15'));
  await openOp('Authentication', '/api/v1/auth/login', 'post');
  await setBody('Type the login body', JSON.stringify({ email: ADMIN.email, password: ADMIN.password }, null, 2));
  d.voiceNow();
  const loginRes = await send('Send login', 'POST', '/api/v1/auth/login', 200);
  const token = (await loginRes.json()).result.token;
  await d.run('Login returned a JWT (checked, not shown)', async () => {
    if (!/^eyJ[\w-]+\.[\w-]+\.[\w-]+$/.test(token)) throw new Error('token shape is wrong');
    const claims = JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString('utf8'));
    if (claims.role !== 'Admin') throw new Error(`role is ${claims.role}`);
    const days = Math.round((claims.exp - claims.nbf) / 86400);
    if (days !== 7) throw new Error(`expiry is ${days} days`);
  });
  await d.endScene();
  await closeModal();

  // ---------- s16: 401 then 201 ----------
  d.scene('s16', need('s16'));
  await openOp('Products', '/api/v1/products', 'post');
  await setBody('Type the product body', JSON.stringify({ title: 'Blue Desk Lamp', description: 'Demo item' }, null, 2));
  d.voiceNow();
  await send('Send create product without a token', 'POST', '/api/v1/products', 401);
  await d.hold(1800);
  await d.click('Expand the Authentication section',
    modal().getByRole('button', { name: /^Authentication/ }).first());
  await d.run('Choose the JWT bearer auth type if not preselected', async () => {
    const tok = modal().getByPlaceholder('Token');
    if (await tok.isVisible().catch(() => false)) return;
    const sel = modal().getByRole('button', { name: /Auth Type|Select/i }).first();
    await d.moveTo(sel); await sel.click(); await d.pause(500);
    await page.getByText('JWTBearerAuth', { exact: true }).first().click();
    await d.pause(600);
  }, async () => { await modal().getByPlaceholder('Token').waitFor({ state: 'visible', timeout: 5000 }); });
  await d.run('Enter the bearer token (blurred on screen)', async () => {
    const inp = modal().getByPlaceholder('Token');
    await d.moveTo(inp); await inp.click(); await inp.fill(token); await d.pause(700);
  }, async () => { if ((await modal().getByPlaceholder('Token').inputValue()) !== token) throw new Error('token not set'); });
  let created;
  const r201 = await send('Send create product with the token', 'POST', '/api/v1/products', 201);
  created = await r201.json();
  await d.run('Response contains the new product id', async () => {
    if (!created.id) throw new Error('no id');
    await modal().getByText('Blue Desk Lamp').first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.endScene();

  // ---------- s17: validation on a slice ----------
  d.scene('s17', need('s17'));
  await setBody('Type a product with an empty title', JSON.stringify({ title: '', description: 'Demo item' }, null, 2));
  d.voiceNow();
  await send('Send create product with empty title', 'POST', '/api/v1/products', 400);
  await d.run('Response shows the title message', async () => {
    await modal().getByText('Title is required.').first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.endScene();
  await closeModal();

  // ---------- s18: JSON, XML, 406 ----------
  d.scene('s18', need('s18'));
  await openOp('Products', '/api/v1/products', 'get');
  d.voiceNow();
  const r200 = await send('Send list products (JSON)', 'GET', '/api/v1/products', 200);
  await d.run('Body is JSON with the new product', async () => {
    if (!(r200.headers()['content-type'] ?? '').includes('json')) throw new Error('not JSON');
    await modal().getByText('Blue Desk Lamp').first().waitFor({ state: 'visible', timeout: 8000 });
  });
  // Scalar's default accept header row is read-only: untick it and add our own Accept header.
  const headerEditors = () => modal().locator('.code-input-lite__editor');
  const setAccept = async (value, first) => {
    await d.run(`Set the Accept header to ${value}`, async () => {
      if (first) {
        const box = modal().getByLabel('Include accept in request');
        await d.moveTo(modal().getByText('accept', { exact: true }).first());
        await box.uncheck({ force: true });
        await d.pause(500);
        await d.moveTo(headerEditors().nth(2)); await headerEditors().nth(2).click();
        await page.keyboard.insertText('Accept');
        await d.moveTo(headerEditors().nth(3)); await headerEditors().nth(3).click();
        await page.keyboard.insertText(value);
      } else {
        await d.moveTo(headerEditors().nth(3)); await headerEditors().nth(3).click();
        await page.keyboard.press('Control+A');
        await page.keyboard.insertText(value);
      }
      await d.pause(1200); // let Scalar commit the header before sending
    });
  };
  await setAccept('application/xml', true);
  const rxml = await send('Send list products (XML)', 'GET', '/api/v1/products', 200);
  await d.run('Body is XML', async () => {
    if (!(rxml.headers()['content-type'] ?? '').includes('xml')) throw new Error('not XML');
    await modal().getByText('GetProductsResponse').first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.hold(1500);
  await setAccept('text/csv', false);
  await send('Send list products (text/csv)', 'GET', '/api/v1/products', 406);
  await d.run('Response status 406 is visible', async () => {
    await modal().getByText(/406/).first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.endScene();
  await closeModal();

  // ---------- s19: users via Dapper (anonymous endpoint) ----------
  d.scene('s19', need('s19'));
  await openOp('Users', '/api/v1/Users', 'get');
  d.voiceNow();
  await send('Send list users', 'GET', '/api/v1/Users', 200);
  await d.run('Response lists the demo admin with role', async () => {
    await modal().getByText(ADMIN.email).first().waitFor({ state: 'visible', timeout: 8000 });
  });
  await d.endScene();
  await closeModal();

  // ---------- s20: delete (the SQL proof is shown in s21 from the viewer recording) ----------
  d.scene('s20', need('s20'));
  await openOp('Products', '/api/v1/products/{id}', 'delete');
  await d.run('Enter the product id in the path parameter', async () => {
    const cell = modal().locator('.code-input-lite__editor').nth(1); // Variables: id value
    await d.moveTo(cell); await cell.click(); await page.keyboard.insertText(created.id); await d.pause(1200);
  });
  d.voiceNow();
  await send('Send delete', 'DELETE', '/api/v1/products/', 204);
  await d.endScene();

  writeFileSync(new URL('../work/j3-created-id.txt', import.meta.url), created.id);
  console.log('JOURNEY PASS J3');
  console.log(log.join('\n'));
} catch (err) {
  console.log(`JOURNEY FAIL J3 - ${err.message.split('\n')[0]}`);
  process.exitCode = 1;
} finally {
  if (process.exitCode !== 1 || true) d.saveMarkers(new URL('../recordings/j3-scalar/markers.json', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1'));
  const video = page.video();
  await context.close();
  if (RECORD && video && process.exitCode !== 1) {
    renameSync(await video.path(), OUT + 'raw.webm');
    console.log('Saved ' + OUT + 'raw.webm');
  }
  await browser.close();
}
