import { spawn, spawnSync } from 'node:child_process';
import { once } from 'node:events';
import { createServer } from 'node:net';
import { mkdtemp, rm, writeFile } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import WebSocket from 'ws';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const serverProject = path.join(root, 'server', 'ProjetoRpg.Server.csproj');
const dotnetVersion = spawnSync('dotnet', ['--version'], { cwd: root, encoding: 'utf8' });
if (dotnetVersion.error || dotnetVersion.status !== 0) {
  console.error('TARGET_TEST_BLOCKED: .NET SDK is unavailable. Install the pinned SDK from global.json, then rerun npm run test:target:server.');
  process.exit(2);
}
console.log(`Using .NET SDK ${dotnetVersion.stdout.trim()}`);

async function availablePort() {
  const probe = createServer();
  probe.listen(0, '127.0.0.1');
  await once(probe, 'listening');
  const { port } = probe.address();
  await new Promise((resolve, reject) => probe.close(error => error ? reject(error) : resolve()));
  return port;
}

function startServer(port, extraEnv = {}) {
  const processHandle = spawn('dotnet', ['run', '--project', serverProject, '--no-launch-profile', '--verbosity', 'quiet'], {
    cwd: root,
    env: { ...process.env, ASPNETCORE_URLS: `http://127.0.0.1:${port}`, ...extraEnv },
    stdio: ['ignore', 'pipe', 'pipe']
  });
  let output = '';
  processHandle.stdout.setEncoding('utf8').on('data', chunk => { output += chunk; });
  processHandle.stderr.setEncoding('utf8').on('data', chunk => { output += chunk; });
  return { processHandle, getOutput: () => output };
}

function waitForClose(processHandle, timeoutMs) {
  return new Promise(resolve => {
    if (processHandle.exitCode !== null || processHandle.signalCode !== null) {
      resolve([processHandle.exitCode, processHandle.signalCode]);
      return;
    }
    const onClose = (code, signal) => {
      clearTimeout(timeout);
      resolve([code, signal]);
    };
    const timeout = setTimeout(() => {
      processHandle.off('close', onClose);
      resolve(null);
    }, timeoutMs);
    processHandle.once('close', onClose);
  });
}

async function stopServer(processHandle) {
  if (processHandle.exitCode !== null || processHandle.signalCode !== null) return;
  processHandle.kill('SIGTERM');
  if (await waitForClose(processHandle, 5000) === null) {
    processHandle.kill('SIGKILL');
    await waitForClose(processHandle, 1000);
  }
}

async function waitForHttp(url, processHandle, readOutput) {
  const deadline = Date.now() + 90000;
  let lastError;
  while (Date.now() < deadline) {
    if (processHandle.exitCode !== null) throw new Error(`Server exited (${processHandle.exitCode}).\n${readOutput()}`);
    try {
      const response = await fetch(url, { signal: AbortSignal.timeout(1000) });
      if (response.ok) return response;
    } catch (error) { lastError = error; }
    await new Promise(resolve => setTimeout(resolve, 200));
  }
  throw new Error(`Timed out waiting for ${url}: ${lastError ?? 'no response'}.\n${readOutput()}`);
}

function websocketProbe(url, initialFrame, expectedMessages, afterMessage) {
  return new Promise((resolve, reject) => {
    const socket = new WebSocket(url);
    const received = [];
    let settled = false;
    const timeout = setTimeout(() => finish(new Error('WebSocket protocol probe timed out.')), 10000);
    function finish(error) {
      if (settled) return;
      settled = true;
      clearTimeout(timeout);
      if (socket.readyState === WebSocket.OPEN) socket.close();
      if (error) reject(error); else resolve(received);
    }
    socket.once('open', () => socket.send(JSON.stringify(initialFrame)));
    socket.on('message', data => {
      let message;
      try { message = JSON.parse(data.toString()); }
      catch { finish(new Error(`Server returned non-JSON WebSocket data: ${data}`)); return; }
      received.push(message);
      if (afterMessage) afterMessage(message, socket);
      if (expectedMessages(received)) finish();
    });
    socket.once('error', error => finish(error));
    socket.once('close', (code, reason) => {
      if (!settled && expectedMessages(received)) finish();
      else if (!settled) finish(new Error(`WebSocket closed early (${code}: ${reason.toString()}).`));
    });
  });
}

let liveServer;
try {
  const port = await availablePort();
  liveServer = startServer(port);
  const baseUrl = `http://127.0.0.1:${port}`;
  await waitForHttp(`${baseUrl}/health/live`, liveServer.processHandle, liveServer.getOutput);

  const catalog = await (await fetch(`${baseUrl}/health/content`)).json();
  if (catalog.contentVersion !== '0.1.0' || catalog.entries !== 0) throw new Error(`Unexpected content catalog response: ${JSON.stringify(catalog)}`);

  const deadline = Date.now() + 3000;
  let tickState;
  do {
    tickState = await (await fetch(`${baseUrl}/health/tick`)).json();
    if (tickState.tickCount > 0) break;
    await new Promise(resolve => setTimeout(resolve, 100));
  } while (Date.now() < deadline);
  if (tickState.tickCount < 1 || tickState.configuredRateHz !== 20 || tickState.persistenceConfigured !== false)
    throw new Error(`Unexpected tick status: ${JSON.stringify(tickState)}`);

  const sampleStartCount = tickState.tickCount;
  const sampleStart = performance.now();
  await new Promise(resolve => setTimeout(resolve, 1000));
  tickState = await (await fetch(`${baseUrl}/health/tick`)).json();
  const sampleElapsedMilliseconds = performance.now() - sampleStart;
  const measuredTickRateHz = (tickState.tickCount - sampleStartCount) * 1000 / sampleElapsedMilliseconds;
  if (measuredTickRateHz < 15 || measuredTickRateHz > 25)
    throw new Error(`Expected the 20 Hz clock to measure within 15–25 Hz; measured ${measuredTickRateHz.toFixed(2)} Hz over ${sampleElapsedMilliseconds.toFixed(0)} ms.`);

  const validMessages = await websocketProbe(`ws://127.0.0.1:${port}/ws`,
    { type: 'Hello', protocolVersion: 1, sequence: 1 },
    messages => messages.some(message => message.type === 'Pong' && message.sequence === 2),
    (message, socket) => {
      if (message.type === 'HelloAck') socket.send(JSON.stringify({ type: 'Ping', protocolVersion: 1, sequence: 2 }));
    });
  if (!validMessages.some(message => message.type === 'HelloAck') || !validMessages.some(message => message.type === 'Pong'))
    throw new Error(`Handshake/ping-pong failed: ${JSON.stringify(validMessages)}`);

  const mismatchMessages = await websocketProbe(`ws://127.0.0.1:${port}/ws`,
    { type: 'Hello', protocolVersion: 999, sequence: 1 },
    messages => messages.some(message => message.type === 'Error'));
  if (mismatchMessages[0]?.detail !== 'protocol_version_or_handshake_invalid')
    throw new Error(`Protocol mismatch was not rejected authoritatively: ${JSON.stringify(mismatchMessages)}`);

  await stopServer(liveServer.processHandle);
  liveServer = undefined;

  const invalidDirectory = await mkdtemp(path.join(os.tmpdir(), 'projetorpg-invalid-content-'));
  let invalidServer;
  try {
    await writeFile(path.join(invalidDirectory, 'manifest.json'), JSON.stringify({ schemaVersion: 999, contentVersion: 'broken', entries: [] }));
    const invalidPort = await availablePort();
    invalidServer = startServer(invalidPort, { GAME_CONTENT_DIRECTORY: invalidDirectory });
    const closeResult = await waitForClose(invalidServer.processHandle, 15000);
    if (closeResult === null) {
      await stopServer(invalidServer.processHandle);
      throw new Error('Server did not reject invalid content during startup.');
    }
    const [exitCode] = closeResult;
    if (exitCode === 0 || !invalidServer.getOutput().includes('Content manifest schemaVersion must be 1.'))
      throw new Error(`Invalid content startup validation did not fail as expected. exit=${exitCode}\n${invalidServer.getOutput()}`);
  } finally {
    if (invalidServer) await stopServer(invalidServer.processHandle);
    await rm(invalidDirectory, { recursive: true, force: true });
  }

  console.log(`SERVER_INTEGRATION_OK: live endpoint, measured clock ${measuredTickRateHz.toFixed(2)} Hz, catalog validation, WebSocket handshake/ping, protocol mismatch rejection, invalid-catalog startup refusal. The clock is not world simulation.`);
} catch (error) {
  console.error(`SERVER_INTEGRATION_FAILED: ${error.stack ?? error}`);
  process.exitCode = 1;
} finally {
  if (liveServer) await stopServer(liveServer.processHandle);
}
