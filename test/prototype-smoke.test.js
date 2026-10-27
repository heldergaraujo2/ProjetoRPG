const test = require('node:test');
const assert = require('node:assert/strict');
const { spawn } = require('node:child_process');
const net = require('node:net');
const path = require('node:path');
const WebSocket = require('ws');

const root = path.resolve(__dirname, '..');

async function unusedPort() {
  const probe = net.createServer();
  await new Promise((resolve, reject) => probe.listen(0, '127.0.0.1', error => error ? reject(error) : resolve()));
  const { port } = probe.address();
  await new Promise((resolve, reject) => probe.close(error => error ? reject(error) : resolve()));
  return port;
}

async function waitForHttp(url, child, timeoutMs = 5000) {
  const deadline = Date.now() + timeoutMs;
  let lastError;
  while (Date.now() < deadline) {
    if (child.exitCode !== null) throw new Error(`server exited early (code ${child.exitCode})`);
    try {
      const response = await fetch(url);
      if (response.ok) return response;
      lastError = new Error(`server returned HTTP ${response.status}`);
    } catch (error) { lastError = error; }
    await new Promise(resolve => setTimeout(resolve, 40));
  }
  throw new Error(`server did not become ready: ${lastError}`);
}

function nextMessage(ws, predicate, timeoutMs = 3000) {
  return new Promise((resolve, reject) => {
    const timer = setTimeout(() => { ws.off('message', onMessage); reject(new Error('WebSocket message timeout')); }, timeoutMs);
    function onMessage(raw) {
      let message;
      try { message = JSON.parse(raw.toString()); } catch { return; }
      if (!predicate(message)) return;
      clearTimeout(timer); ws.off('message', onMessage); resolve(message);
    }
    ws.on('message', onMessage);
  });
}

test('browser client and authoritative prototype server pass HTTP/WebSocket smoke checks', { timeout: 10000 }, async t => {
  const port = await unusedPort();
  const base = `http://127.0.0.1:${port}`;
  const child = spawn(process.execPath, ['server.js'], {
    cwd: root,
    env: { ...process.env, PORT: String(port) },
    stdio: ['ignore', 'ignore', 'pipe']
  });
  let stderr = '';
  child.stderr.setEncoding('utf8');
  child.stderr.on('data', chunk => { stderr += chunk; });
  t.after(() => { if (child.exitCode === null) child.kill('SIGTERM'); });

  const htmlResponse = await waitForHttp(base, child);
  const html = await htmlResponse.text();
  assert.match(html, /<canvas id="world"/);
  assert.match(html, /src="\/app\.js"/);

  const clientResponse = await fetch(`${base}/app.js`);
  assert.equal(clientResponse.status, 200);
  const clientSource = await clientResponse.text();
  assert.match(clientSource, /requestAnimationFrame\(frame\)/);
  assert.match(clientSource, /new WebSocket/);

  const ws = new WebSocket(`ws://127.0.0.1:${port}`);
  t.after(() => { if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) ws.terminate(); });
  const welcome = new Promise((resolve, reject) => {
    const timer = setTimeout(() => reject(new Error(`welcome timeout. Server stderr: ${stderr}`)), 3000);
    ws.once('error', error => { clearTimeout(timer); reject(error); });
    // A client-supplied destination must not grant a free map change.
    ws.once('open', () => ws.send(JSON.stringify({ type: 'join', name: 'Teste', classId: 'cacador', mapId: 'ruinas' })));
    ws.on('message', raw => {
      const message = JSON.parse(raw.toString());
      if (message.type === 'welcome') { clearTimeout(timer); resolve(message); }
    });
  });
  const message = await welcome;
  assert.equal(message.player.name, 'Teste');
  assert.equal(message.player.classId, 'cacador');
  assert.equal(message.map.id, 'campina');
  assert.ok(Array.isArray(message.map.mobs));

  const second = new WebSocket(`ws://127.0.0.1:${port}`);
  t.after(() => { if (second.readyState === WebSocket.OPEN || second.readyState === WebSocket.CONNECTING) second.terminate(); });
  const secondWelcome = nextMessage(second, item => item.type === 'welcome');
  second.once('open', () => second.send(JSON.stringify({ type: 'join', name: 'Teste2', classId: 'guardiao' })));
  const secondPlayer = await secondWelcome;
  assert.equal(secondPlayer.player.name, 'Teste2');

  const sharedWorld = await nextMessage(ws, item => item.type === 'snapshot' && item.players.length === 2);
  assert.ok(sharedWorld.players.some(item => item.name === 'Teste2'));
  const sharedChat = nextMessage(second, item => item.type === 'chat' && item.text === 'olá mundo');
  ws.send(JSON.stringify({ type: 'chat', text: 'olá mundo' }));
  const chat = await sharedChat;
  assert.equal(chat.name, 'Teste');
});
