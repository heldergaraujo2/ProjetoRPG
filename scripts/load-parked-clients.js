import WebSocket from 'ws';

const baseUrl = process.env.PROJETORPG_LOAD_URL ?? 'http://127.0.0.1:5050';
const clientCount = Number.parseInt(process.env.PROJETORPG_LOAD_CLIENTS ?? '100', 10);
const durationSeconds = Number.parseInt(process.env.PROJETORPG_LOAD_DURATION_SECONDS ?? '60', 10);
if (!Number.isInteger(clientCount) || clientCount < 1 || clientCount > 1000 ||
    !Number.isInteger(durationSeconds) || durationSeconds < 1 || durationSeconds > 600) {
  console.error('LOAD_TEST_INVALID: clients must be 1..1000 and duration must be 1..600 seconds.');
  process.exit(2);
}

const wsUrl = new URL('/ws', baseUrl);
wsUrl.protocol = wsUrl.protocol === 'https:' ? 'wss:' : 'ws:';
const sockets = [];

function connectAndHandshake(index) {
  return new Promise((resolve, reject) => {
    const socket = new WebSocket(wsUrl);
    sockets.push(socket);
    let settled = false;
    const timeout = setTimeout(() => finish(new Error(`client ${index} handshake timed out`)), 15000);
    function finish(error) {
      if (settled) return;
      settled = true;
      clearTimeout(timeout);
      if (error) reject(error); else resolve(socket);
    }
    socket.once('open', () => socket.send(JSON.stringify({ type: 'Hello', protocolVersion: 2, sequence: 1 })));
    socket.once('message', data => {
      let message;
      try { message = JSON.parse(data.toString()); }
      catch { finish(new Error(`client ${index} received malformed JSON`)); return; }
      if (message.type !== 'HelloAck' || message.sequence !== 1) {
        finish(new Error(`client ${index} handshake was rejected: ${JSON.stringify(message)}`));
        return;
      }
      finish();
    });
    socket.once('error', error => finish(new Error(`client ${index}: ${error.message}`)));
    socket.once('close', (code, reason) => {
      if (!settled) finish(new Error(`client ${index} disconnected during handshake (${code}: ${reason.toString()})`));
    });
  });
}

let succeeded = false;
try {
  const live = await fetch(new URL('/health/live', baseUrl), { signal: AbortSignal.timeout(5000) });
  if (!live.ok) throw new Error(`server liveness returned HTTP ${live.status}`);
  const database = await fetch(new URL('/health/database', baseUrl), { signal: AbortSignal.timeout(5000) });
  if (!database.ok) throw new Error(`database is not available (HTTP ${database.status}); parked-client test requires a healthy local server`);

  await Promise.all(Array.from({ length: clientCount }, (_, index) => connectAndHandshake(index + 1)));
  await new Promise(resolve => setTimeout(resolve, durationSeconds * 1000));
  if (sockets.some(socket => socket.readyState !== WebSocket.OPEN))
    throw new Error('one or more parked WebSocket clients disconnected before the hold completed');
  succeeded = true;
  console.log(`PARKED_CLIENT_LOAD_OK: ${clientCount} protocol handshakes held idle for ${durationSeconds}s. This measures connection handling only, not authenticated players, gameplay, world simulation, or production capacity.`);
} catch (error) {
  console.error(`PARKED_CLIENT_LOAD_FAILED: ${error.stack ?? error}`);
  process.exitCode = 1;
} finally {
  for (const socket of sockets) {
    if (socket.readyState === WebSocket.OPEN) socket.close();
    else if (socket.readyState === WebSocket.CONNECTING) socket.terminate();
  }
  if (!succeeded && process.exitCode === undefined) process.exitCode = 1;
}
